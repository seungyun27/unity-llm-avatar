const fs = require('fs');
const path = require('path');

function countWords(text) {
    if (typeof text !== 'string') return 0;
    const wc = text.trim().split(/\s+/).filter(word => word.length > 0).length;
    if (text.startsWith('Analysis: ') || text.startsWith('Answer: ')) {
        return wc - 1;
    }
    return wc;
}

function analyzeMessages(jsonData) {
    let messages = [];

    if (jsonData.chat && Array.isArray(jsonData.chat)) {

        const reduced = jsonData.chat.reduce((acc, current, index) => {
            if (index % 2 === 0) {
                acc.push({ user: current.content });
            } else {
                acc[acc.length - 1] = {
                    ...acc[acc.length - 1],
                    response: current.content
                };
            }
            return acc;
        }, []);

        reduced.forEach((chatObject, index) => {
            let data = {};
            const parts = chatObject.response.split('\n');

            if (parts.length >= 2) {
                const [analysis, answer] = parts;
                
                data = {
                    ...chatObject,
                    analysis: {
                        content: analysis,
                        count: countWords(analysis)
                    },
                    answer: {
                        content: answer,
                        count: countWords(answer),
                    },
                };
            } else {
                // Malformatted response
                data = {
                    ...chatObject,
                    analysis: null,
                    answer: null,
                };
            }
            messages.push(data);
        });
    }

    return messages;
}

function processJSONFile(filePath) {
    try {
        const jsonContent = fs.readFileSync(filePath, 'utf8');
        const jsonData = JSON.parse(jsonContent);

        const analysisResult = analyzeMessages(jsonData);

        const results = {
            file: path.basename(filePath),
            messages: analysisResult,
        };

        // Print structured results
        console.log(JSON.stringify(results, null, 2));

        // Save results to file
        const outputPath = filePath.replace('.json', '-word-count.json');
        fs.writeFileSync(outputPath, JSON.stringify(results, null, 2));
        console.log(`\n--- Results saved to: ${outputPath} ---`);

        return results;

    } catch (error) {
        console.error(`Error processing file ${filePath}:`, error.message);
        return null;
    }
}

// Main execution
const args = process.argv.slice(2);
if (args.length === 0) {
    console.log('Usage: node word-counter.js <json-file-path>');
    console.log('Example: node word-counter.js llama3.2-3b-temp-0.6.json');
    process.exit(1);
}

const filePath = args[0];

if (!fs.existsSync(filePath)) {
    console.error(`File not found: ${filePath}`);
    process.exit(1);
}

processJSONFile(filePath);