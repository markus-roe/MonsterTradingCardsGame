SELECT * FROM users;

SELECT * FROM cards;
	
SELECT * FROM packages;

SELECT * FROM package_cards;

SELECT uc.*, c.name, c.damage, u.username FROM user_cards uc JOIN cards c on uc.cardid = c.id JOIN users u on uc.userid = u.id
WHERE u.username = 'altenhof';

SELECT uc.*, u.username FROM user_cards uc JOIN users u ON uc.userid = u.id WHERE cardid = '1cb6ab86-bdb2-47e5-b6e4-68c5ab389334';

SELECT uc.*, u.username FROM user_cards uc JOIN users u ON uc.userid = u.id WHERE cardid = '951e886a-0fbf-425d-8df5-af2ee4830d85';



SELECT * FROM CARDS WHERE id = '1cb6ab86-bdb2-47e5-b6e4-68c5ab389334';

SELECT * FROM CARDS WHERE id = '845f0dc7-37d0-426e-994e-43fc3ac83c08';



----------delete----------
DELETE FROM tradings;
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

INSERT INTO tradings (id, cardtotrade, type, minimumdamage, userid) VALUES ('845f0dc7-37d0-426e-994e-43fc3ac83c08', '845f0dc7-37d0-426e-994e-43fc3ac83c08', 'monster', 15, 6);

