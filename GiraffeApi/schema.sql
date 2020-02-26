CREATE TABLE users (
    user_id serial PRIMARY KEY,
    name VARCHAR(50) NOT NULL,
    email VARCHAR(200) NULL,
    created_date DATE NOT NULL
);
