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



CREATE TABLE IF NOT EXIST userstats (
    userid INTEGER NOT NULL,
    wins INTEGER DEFAULT 0,
    losses INTEGER DEFAULT 0,
    CONSTRAINT pk_userstats PRIMARY KEY (userid),
    CONSTRAINT fk_user
        FOREIGN KEY(userid) 
        REFERENCES users(id)
);

CREATE VIEW user_statsview AS
SELECT 
	u.id as userid,
    u.name,
    u.elo,
    us.wins,
    us.losses
FROM 
    users u
JOIN 
    userstats us ON u.id = us.userid;








