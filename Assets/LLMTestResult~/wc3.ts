import { mkConfig, generateCsv, asString } from 'export-to-csv';
import { XMLParser } from 'fast-xml-parser';
import path from 'node:path';

const xmlParser = new XMLParser({
    textNodeName: '$text',
});

interface MessageData {
    role: string,
    content: string
}

interface ChatData {
    chat: Array<MessageData>,
}

interface ResultData {
    modelName: string,
    temperature: number,
    user: string,
    response: string,
    analysis_content?: string,
    analysis_count?: number,
    answer_content?: string,
    answer_count?: number,
}

function countWords(text: string) {
    if (typeof text !== 'string') return 0;
    const wc = text.trim().split(/\s+/).filter(word => word.length > 0).length;
    if (text.startsWith('Analysis: ') || text.startsWith('Answer: ')) {
        return wc - 1;
    }
    return wc;
}

function analyzeMessages(jsonData: any, modelName: string, temperature: number) {
    let messages: Array<ResultData> = [];

    if (jsonData.chat && Array.isArray(jsonData.chat)) {
        const reduced = jsonData.chat.reduce((acc: ResultData[], current: any, index: number) => {
            if (index % 2 === 0) {
                acc.push({
                    modelName,
                    temperature,
                    user: current.content,
                    response: ''
                });
            } else {
                acc[acc.length - 1] = {
                    ...acc[acc.length - 1]!,
                    response: current.content,
                };
            }
            return acc;
        }, []);

        reduced.forEach((chatObject: any) => {
            chatObject.user = xmlParser.parse(chatObject.user).request;
            let data: ResultData = { ...chatObject };

            let analysis = '', answer = '';
            try {
                const p = xmlParser.parse(chatObject.response).response;
                analysis = (typeof p.analysis === 'string') ? p.analysis : p.analysis.$text;
                answer = (typeof p.answer === 'string') ? p.answer : p.answer.$text;
            } catch (error: any) {
                console.error('error parsing XML: ', typeof error, error);
                // process.exit(1);
            }

            if (analysis && answer) {
                data = {
                    ...chatObject,
                    analysis_content: analysis,
                    analysis_count: countWords(analysis!),
                    answer_content: answer,
                    answer_count: countWords(answer!),
                };
            }

            messages.push(data);
        });
    }

    return messages;
}

async function processJSONFile(file: Bun.BunFile) {
    try {
        const jsonData = await file.json();
        const [ modelName, temperatureStr, promptType, timestamp ] = path.basename(file.name!).replace('.json', '').split('___');
        // temp-0.6 -> [ temp, 0.6 ]
        const temperature = Number.parseFloat(temperatureStr?.split('-')[1]!);
        const analysisResult = analyzeMessages(jsonData, modelName ?? 'UNKNOWN_MODEL', temperature);

        const csvConfig = mkConfig({
            useKeysAsHeaders: true,
        });

        try {
            const csv = generateCsv(csvConfig)(analysisResult as any[]);
            const csvPath = file.name!.replace('.json', '-word-count.csv');
            Bun.write(csvPath, asString(csv));
            console.log('Saved as csv: ', csvPath);
        } catch (error: any) {
            console.error('error processing CSV: ', typeof error, error);
        }

        // Save results to file
        // const outputPath = file.name!.replace('.json', '-word-count.json');
        // Bun.write(outputPath, JSON.stringify(analysisResult, null, 2));
        // console.log(`\n--- Results saved to: ${outputPath} ---`);

        return analysisResult;

    } catch (error: any) {
        console.error(`Error processing file ${filePath}:`, error.message);
        return null;
    }
}

const args = process.argv.slice(2);
if (args.length === 0) {
    console.log('Usage: node word-counter.js <json-file-path>');
    console.log('Example: node word-counter.js llama3.2-3b-temp-0.6.json');
    process.exit(1);
}

const filePath = args[0];
const file = Bun.file(filePath!);
if (!await file.exists()) {
    console.error(`File not found: ${filePath}`);
    process.exit(1);
}

processJSONFile(file);