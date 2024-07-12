-- Bảng lưu thông tin về các cổ phiếu
CREATE TABLE stocks (
    stock_id VARCHAR(10) PRIMARY KEY,
    company_name VARCHAR(255) NOT NULL,
    current_price DECIMAL(10, 2) NOT NULL,
    volume INT NOT NULL,
    update_date DATE NOT NULL
);

INSERT INTO stocks (stock_id, company_name, current_price, volume, update_date) VALUES
('APL', 'Apple Inc.', 150.25, 0, '2024-05-20'),
('MSF', 'Microsoft Corp.', 310.50, 0, '2024-05-20'),
('AMZ', 'Amazon.com Inc.', 3200.75, 0, '2024-05-20'),
('TSL', 'Tesla Inc.', 725.65, 0, '2024-05-20'),
('ALP', 'Alphabet Inc.', 2750.40, 0, '2024-05-20'),
('FPT', 'FPT Corporation', 75.80, 0, '2024-05-20');

-- Bảng lưu thông tin về người dùng và số dư tài khoản của họ
CREATE TABLE users (
    user_id VARCHAR(10) PRIMARY KEY,
    username VARCHAR(50) UNIQUE NOT NULL,
    password VARCHAR(100) NOT NULL,
    phone VARCHAR(15) UNIQUE,
    account_balance DECIMAL(10, 2) NOT NULL,
    role ENUM('Admin', 'User') DEFAULT 'User' -- Added role column with ENUM type and default value 'User'
);

INSERT INTO users (user_id, username, password, phone, account_balance, role) VALUES
('1', 'user1', 'hashpassword', '123456789', 99999999.50, 'Admin');

INSERT INTO users (user_id, username, password, phone, account_balance) VALUES
('2', 'user2', 'hashpassword', '987654321', 99999999.50),
('3', 'user3', 'hashpassword', '555666777', 99999999.50);

-- Bảng lưu thông tin về các giao dịch của người dùng với các cổ phiếu
CREATE TABLE transactions (
    transaction_id INT AUTO_INCREMENT PRIMARY KEY,
    sender_id VARCHAR(10) NOT NULL,
    receiver_id VARCHAR(10),
    stock_id VARCHAR(10) NOT NULL,
    quantity INT NOT NULL,
    transaction_type ENUM('buy', 'sell') NOT NULL,
    current_price DECIMAL(10, 2) NOT NULL,
    transaction_date DATETIME NOT NULL,
    FOREIGN KEY (sender_id) REFERENCES users(user_id),
    FOREIGN KEY (receiver_id) REFERENCES users(user_id),
    FOREIGN KEY (stock_id) REFERENCES stocks(stock_id)
);

CREATE TABLE sell_buy_requests (
    request_id INT AUTO_INCREMENT PRIMARY KEY,
    user_id VARCHAR(10),
    stock_id VARCHAR(10),
    quantity INT NOT NULL,
    price DECIMAL(10, 2) NOT NULL,
    request_date DATETIME NOT NULL,
    request_type ENUM('buy', 'sell') NOT NULL, -- Loại giao dịch: mua hoặc bán
    FOREIGN KEY (user_id) REFERENCES users(user_id),
    FOREIGN KEY (stock_id) REFERENCES stocks(stock_id)
);

CREATE TABLE RefreshToken (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    user_id VARCHAR(10),
    Token TEXT,
    JwtId VARCHAR(255),
    IsAccount BOOLEAN,
    IsRevoked BOOLEAN,
    IssueAt DATETIME,
    ExpireAt DATETIME,
    FOREIGN KEY (user_id) REFERENCES users(user_id)
);