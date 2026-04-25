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


const express = require('express');
const router = express.Router();

let users = [];

router.post('/register', (req, res) => {
  const { email, password } = req.body;

  if (!email || !password) {
    return res.status(400).json({ error: 'Email and password required' });
  }

  const newUser = {
    id: Date.now().toString(),
    email,
    password, // (пізніше тут буде хешування)
    role: 'user'
  };

  users.push(newUser);

  res.status(201).json({ message: 'User registered', user: newUser });
});

module.exports = router;