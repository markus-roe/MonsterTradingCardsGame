SELECT * FROM users;

SELECT * FROM cards;

SELECT * FROM user_cards;
	
SELECT * FROM packages;

SELECT * FROM package_cards;

SELECT uc.*, c.name, c.damage, u.username FROM user_cards uc JOIN cards c on uc.cardid = c.id JOIN users u on uc.userid = u.id;

SELECT uc.*, u.username FROM user_cards uc JOIN users u ON uc.userid = u.id WHERE uc.cardid = 'd181b9e4-52e0-4d98-aee2-8f471913d85e';

SELECT * from user_cards WHERE cardid = 'd181b9e4-52e0-4d98-aee2-8f471913d85e';


----------delete----------
DELETE FROM tradings;
DELETE FROM users WHERE username = 'altenhof';
DELETE FROM users WHERE username = 'kienboec';
DELETE FROM users WHERE username = 'admin';
DELETE FROM userstats;
DELETE FROM package_cards;
DELETE FROM packages;
DELETE FROM cards;
DELETE FROM user_cards;


---

SELECT * FROM userstats;

SELECT * FROM user_statsview;

SELECT * FROM tradings;


