using MonsterTradingCardsGame.Repositories;
using MonsterTradingCardsGame.Server;
using MonsterTradingCardsGame.Interfaces;
using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Services.Interfaces;
using MonsterTradingCardsGame.Utils.UserStats;
using Moq;

namespace MonsterTradingCardsGame.Tests.Repositories
{



    [TestFixture]
    internal class UserRepositoryTests
    {

        private Mock<IUserRepository> _mockUserRepository;
        private Mock<IAuthenticationService> _mockAuthService;
        private Mock<ICardRepository> _mockCardRepository;
        private Mock<IHttpServerEventArguments> _mockHttpEventArguments;
        private UserRepository _userRepository;

        //Setup for the tests
        [SetUp]
        public void Setup()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockAuthService = new Mock<IAuthenticationService>();
            _mockCardRepository = new Mock<ICardRepository>();
            _mockHttpEventArguments = new Mock<IHttpServerEventArguments>();

            _userRepository = new UserRepository(_mockCardRepository.Object);

            User testuser = new User()
            {
                Username = "testuser",
                Password = "password"
            };

            _userRepository.DeleteUser(testuser);
        }

        [Test]
        public void GetUserByUsername_ExistingUsername_ReturnsUser()
        {
            // Arrange
            string username = "kienboec";

            // Act
            User? user = _userRepository.GetUserByUsername(username);

            // Assert
            Assert.IsNotNull(user);
            Assert.That(user.Username, Is.EqualTo(username));
        }

        [Test]
        public void GetUserByUsername_NonExistingUsername_ReturnsNull()
        {
            // Arrange
            string username = "nonexistinguser";

            // Act
            User? user = _userRepository.GetUserByUsername(username);

            // Assert
            Assert.IsNull(user);
        }

        //test for delete user
        [Test]
        public void DeleteUser_ExistingUser_ReturnsTrue()
        {
            // Arrange
            User user = new User()
            {
                Username = "testuser",
                Password = "password"
            };

            _userRepository.SaveUser(user);

            // Act
            bool result = _userRepository.DeleteUser(user);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void DeleteUser_NonExistingUser_ReturnsFalse()
        {
            // Arrange
            User user = new User()
            {
                Username = "nonexistinguser",
                Password = "password"
            };

            // Act
            bool result = _userRepository.DeleteUser(user);

            // Assert
            Assert.IsFalse(result);
        }



        [Test]
        public void DeleteUser_ActuallyDeletesUser()
        {
            // Arrange
            User user = new User()
            {
                Username = "testuser",
                Password = "password"
            };

            // Act
            _userRepository.DeleteUser(user);

            // Assert
            Assert.IsNull(_userRepository.GetUserByUsername("testuser"));
        }

        //test for update user
        [Test]
        public void UpdateUser_ExistingUser_ReturnsTrue()
        {
            // Arrange
            User user = new User()
            {
                Username = "kienboec",
                Name = "Kienboec",
                Bio = "I am a cool guy",
                Image = ":----D",
                Coins = 100,
                Elo = 1000
            };

            // Act
            bool result = _userRepository.UpdateUser(user);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void UpdateNonExistingUser_ReturnsFalse()
        {
            // Arrange
            User user = new User()
            {
                Username = "nonexistinguser",
                Name = "Kienboec",
                Bio = "I am a cool guy",
                Image = ":----D",
                Coins = 100,
                Elo = 1000
            };

            // Act
            bool result = _userRepository.UpdateUser(user);

            // Assert
            Assert.IsFalse(result);
        }


        [Test]
        public void AddWin_ExistingUser_ChangesEloAndReturnsTrue()
        {
            // Arrange
            User user = new User()
            {
                Username = "testuser",
                Password = "testpassword",
            };


            int? userId = _userRepository.SaveUser(user);

            user.Id = userId.Value;

            // Act
            bool result = _userRepository.AddWin(user);

            user = _userRepository.GetUserById(userId.Value);

            // Assert
            Assert.IsTrue(result);
            Assert.That(user.Elo, Is.EqualTo(103));
        }

        [Test]
        public void AddWin_NonExistingUser_ReturnsFalse()
        {
            // Arrange
            User user = new User()
            {
                Id = 999,
                Username = "nonexistinguser",
                Password = "password"
            };

            // Act
            bool result = _userRepository.AddWin(user);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void AddLoss_ExistingUser_ChangesEloAndReturnsFalse()
        {

            // Arrange
            User user = new User()
            {
                Username = "testuser",
                Password = "testpassword",
            };


            int? userId = _userRepository.SaveUser(user);

            user.Id = userId.Value;

            // Act
            bool result = _userRepository.AddLoss(user);

            user = _userRepository.GetUserById(userId.Value);

            // Assert
            Assert.IsTrue(result);
            Assert.That(user.Elo, Is.EqualTo(95));
        }

        [Test]
        public void AddLoss_NonExistingUser_ReturnsFalse()
        {
            // Arrange
            User user = new User()
            {
                Id = 999,
                Username = "nonexistinguser",
                Password = "password"
            };

            // Act
            bool result = _userRepository.AddLoss(user);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void GetUserById_ExistingId_ReturnsUser()
        {
            // Arrange
            User newUser = new User()
            {
                Username = "testuser",
                Password = "password"
            };

            //save new user
            int? userId = _userRepository.SaveUser(newUser);


            // Act
            User? user = _userRepository.GetUserById(userId.Value);

            // Assert
            Assert.IsNotNull(user);
            Assert.That(user.Id, Is.EqualTo(userId));
        }

        [Test]
        public void GetUserById_NonExistingId_ReturnsNull()
        {
            // Arrange
            int id = 999;

            // Act
            User? user = _userRepository.GetUserById(id);

            // Assert
            Assert.IsNull(user);
        }

        [Test]
        public void GetStatsByUser_ExistingUser_ReturnsUserStats()
        {
            // Arrange
            User user = new User()
            {
                Username = "testuser"
            };

            //save new user
            int? userId = _userRepository.SaveUser(user);

            user.Id = userId.Value;

            //update user name
            user.Name = "Test User";
            _userRepository.UpdateUser(user);

            // Act
            UserStats? stats = _userRepository.GetStatsByUser(user);

            // Assert
            Assert.IsNotNull(stats);
            Assert.That(stats.Name, Is.EqualTo("Test User"));
            Assert.That(stats.Elo, Is.EqualTo(100));
            Assert.That(stats.Wins, Is.EqualTo(0));
            Assert.That(stats.Losses, Is.EqualTo(0));

        }

        [Test]
        public void GetStatsByUser_NonExistingUser_ReturnsNull()
        {
            // Arrange
            User user = new User()
            {
                Id = 999,
                Username = "nonexistinguser",
                Password = "password"
            };

            // Act
            UserStats? stats = _userRepository.GetStatsByUser(user);

            // Assert
            Assert.IsNull(stats);
        }

    }
}