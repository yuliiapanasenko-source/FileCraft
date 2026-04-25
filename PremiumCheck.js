// Функція для перевірки преміум доступу користувача
function hasPremiumAccess(user) {
  // Перевіряємо, чи існує користувач і чи має він преміум статус
  return user && user.premium === true;
}

module.exports = { hasPremiumAccess };