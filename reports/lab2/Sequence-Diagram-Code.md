@startuml
actor "Користувач" as User
participant "Web UI" as UI
participant "Сервер" as Server
participant "Модуль\nобробки файлів" as Processor
database "База даних" as DB

User -> UI : Обрати файл і натиснути\n«Завантажити»
activate UI

UI -> Server : uploadFile(file)
activate Server

Server -> DB : Перевірка користувача
activate DB
DB --> Server : Користувач валідний
deactivate DB

alt Файл коректний
    Server -> Processor : processFile(file)
    activate Processor

    loop Обробка даних
        Processor -> Processor : Аналіз файлу
    end

    Processor --> Server : processedReport
    deactivate Processor

    Server -> DB : Зберегти інформацію\nпро файл і звіт
    activate DB
    DB --> Server : OK
    deactivate DB

    Server --> UI : Звіт готовий
else Помилка файлу
    Server --> UI : Повідомлення про помилку
end

deactivate Server

UI --> User : Відобразити результат /\nдати посилання на звіт
deactivate UI

@enduml