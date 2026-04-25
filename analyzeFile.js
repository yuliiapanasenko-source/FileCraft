/* обробки файлу*/
function analyzeFile(text) {
    return {
        summary: text.substring(0, 100), // короткий витяг
        wordCount: text.split(' ').length
    };
}
const input = "This is a simple test text for FileCraft project";
const result = analyzeFile(input);

console.log("Result:", result);