import { mkConfig, generateCsv, asString } from 'export-to-csv';
import path from 'node:path';

interface MessageData {
    role: string,
    content: string
}

interface ChatData {
    chat: Array<MessageData>,
}

interface ResultData {
    modelName: string,
    temperature?: number,
    user: string,
    response: string,
    review_content?: string,
    review_count?: number,
    answer_content?: string,
    answer_count?: number,
}

function countWords(text: string) {
    if (typeof text !== 'string') return 0;

    text = text.toLowerCase();
    const wc = text.trim().split(/\s+/).filter(word => word.length > 0).length;
    if (text.startsWith('review: ') || text.startsWith('reply: ')) {
        return wc - 1;
    }
    return wc;
}

function analyzeMessages(jsonData: any, modelName: string, temperature?: number) {
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
            let data: ResultData = {
                ...chatObject,
                review_content: '',
                review_count: 0,
                answer_content: '',
                answer_count: 0,
            };

            let response: string = chatObject.response.replace(/\r?\n\r?\n/g, '\n');

            // Extract content from code blocks if wrapped in triple backticks
            const codeBlockMatch = response.match(/```(?:\w+)?\s*\n([\s\S]*?)\n```/);
            if (codeBlockMatch) {
                response = codeBlockMatch[1]!.trim();
            }

            const parts: string[] = response.split('\n');

            if (parts.length === 2) {
                let [review, answer] = parts;

                review = review?.split(':')[1]?.trim();
                answer = answer?.split(':')[1]?.trim();

                data = {
                    ...chatObject,
                    review_content: review,
                    review_count: countWords(review!),
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
        const modelName = path.basename(file.name!).replace('.json', '');
        let [name, temp, _] = modelName.split('___');
        name = name?.split('.gguf')[0];
        temp = temp?.split('-')[1];
        const reviewResult = analyzeMessages(jsonData, name ?? 'UNKNOWN_MODEL', parseFloat(temp));

        // Print structured results
        console.log(JSON.stringify(reviewResult, null, 2).slice(0, 20) + "...");

        const csvConfig = mkConfig({
            useKeysAsHeaders: true,
        });
        const csv = generateCsv(csvConfig)(reviewResult as any[]);
        const csvPath = `${name}_wc.csv`;
        Bun.write(csvPath, asString(csv));
        console.log('Saved as csv: ', csvPath);

        return reviewResult;

    } catch (error: any) {
        console.error(`Error processing file ${filePath}:`, error.message);
        return null;
    }
}

const args = process.argv.slice(2);
// if (args.length === 0) {
//     console.log('Usage: node word-counter.js <json-file-path>');
//     console.log('Example: node word-counter.js llama3.2-3b-temp-0.6.json');
//     process.exit(1);
// }

const filePath = args[0] ?? 'gemma-3-1b-it-Q4_K_M.gguf___temp-0.4___conveniencestore___20251105-163406.json';
const file = Bun.file(filePath!);
if (!await file.exists()) {
    console.error(`File not found: ${filePath}`);
    process.exit(1);
}

processJSONFile(file);