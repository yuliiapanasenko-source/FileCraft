const express = require('express');
const router = express.Router();

// "обробка" файлу (імітація ШІ)
function analyzeFile(text) {
  return {
    summary: text.substring(0, 100), // короткий витяг
    wordCount: text.split(' ').length
  };
}

// endpoint для обробки файлу
router.post('/analyze', (req, res) => {
  const { content } = req.body;

  if (!content) {
    return res.status(400).json({ error: 'No content provided' });
  }

  const result = analyzeFile(content);

  res.json({
    message: 'File processed successfully',
    data: result
  });
});

module.exports = router;