SELECT * FROM users;

SELECT * FROM cards;
	
SELECT * FROM packages;

SELECT * FROM package_cards;

SELECT * FROM user_cards;

SELECT uc.*, u.username FROM user_cards uc JOIN users u ON uc.userid = u.id ORDER by userid ASC;




----------delete----------
DELETE FROM users WHERE username = 'altenhof';
DELETE FROM users WHERE username = 'kienboec';
DELETE FROM users WHERE username = 'admin';
DELETE FROM package_cards;
DELETE FROM packages;
DELETE FROM cards;
DELETE FROM user_cards;


---

SELECT * FROM userstats;

SELECT * FROM user_statsview;

CREATE TABLE tradings (
    id VARCHAR(255) PRIMARY KEY,
    cardtotrade VARCHAR(255) NOT NULL,
    type VARCHAR(255) NOT NULL,
    minimumdamage FLOAT NOT NULL,
    userid int,
    FOREIGN KEY (cardtotrade) REFERENCES cards(id),
    FOREIGN KEY (userid) REFERENCES users(id)
);

SELECT * FROM tradings;

INSERT INTO tradings (id, cardid, type, minimumdamage) VALUES ('845f0dc7-37d0-426e-994e-43fc3ac83c08', '845f0dc7-37d0-426e-994e-43fc3ac83c08', 'monster', 15);
