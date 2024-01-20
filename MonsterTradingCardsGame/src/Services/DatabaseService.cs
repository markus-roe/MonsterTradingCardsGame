using Npgsql;
using Microsoft.Extensions.Configuration;
using MonsterTradingCardsGame.Services.Interfaces;

public class DatabaseInitializationService : IDatabaseInitializationService
{
    private readonly IConfiguration _configuration;

    public DatabaseInitializationService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task InitializeOrResetDatabase(bool resetDatabase)
    {
        try
        {
            var connectionString = _configuration.GetConnectionString("postgres");

            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException("Connection string could not be resolved.");


            bool dbExists = await CheckDatabaseExists(connectionString);
            if (!dbExists)
            {
                await CreateDatabase(connectionString);
            }

            if (resetDatabase)
            {
                await ResetDatabase(connectionString);
            }
            else
            {
                await InitializeDatabase(connectionString);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing or resetting database: {ex.Message}");
            throw;
        }
    }

    private async Task InitializeDatabase(string connectionString)
    {
        using (var connection = new NpgsqlConnection(connectionString))
        {
            await connection.OpenAsync();
            await CreateTables(connection);
        }
    }

    //reset db tables method
    private async Task ResetDatabase(string connectionString)
    {
        try
        {

            // Open a new connection to the newly created database
            using (var connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync();
                await ResetTables(connection);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing database: {ex.Message}");
            throw;
        }
    }

    //reset tables method
    // delete all rows from all tables
    private async Task ResetTables(NpgsqlConnection connection)
    {
        try
        {
            await ResetPackageCardsTable(connection);
            await ResetPackagesTable(connection);
            await ResetUserCardsTable(connection);
            await ResetCardsTable(connection);
            await ResetUserStatsTable(connection);
            await ResetUsersTable(connection);
            await ResetTradingsTable(connection);
            await ResetSessionsTable(connection);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reseting tables: {ex.Message}");
            throw;
        }
    }

    private async Task ResetCardsTable(NpgsqlConnection connection)
    {
        var cmdText = @"DELETE FROM public.cards;";
        await ExecuteCommand(connection, cmdText);
    }

    private async Task ResetPackageCardsTable(NpgsqlConnection connection)
    {
        try
        {
            var cmdText = @"DELETE FROM public.package_cards;";
            await ExecuteCommand(connection, cmdText);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating package_cards table: {ex.Message}");
            throw;
        }
    }

    private async Task ResetPackagesTable(NpgsqlConnection connection)
    {
        try
        {
            var cmdText = @"DELETE FROM public.packages;";
            await ExecuteCommand(connection, cmdText);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating packages table: {ex.Message}");
            throw;
        }
    }

    private async Task ResetUsersTable(NpgsqlConnection connection)
    {
        try
        {
            var cmdText = @"DELETE FROM public.users;";
            await ExecuteCommand(connection, cmdText);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating users table: {ex.Message}");
            throw;
        }
    }

    private async Task ResetUserStatsTable(NpgsqlConnection connection)
    {
        try
        {
            var cmdText = @"DELETE FROM public.userstats;";
            await ExecuteCommand(connection, cmdText);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating userstats table: {ex.Message}");
            throw;
        }
    }

    private async Task ResetUserCardsTable(NpgsqlConnection connection)
    {
        try
        {
            var cmdText = @"DELETE FROM public.user_cards;";
            await ExecuteCommand(connection, cmdText);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating user_cards table: {ex.Message}");
            throw;
        }
    }

    private async Task ResetTradingsTable(NpgsqlConnection connection)
    {
        try
        {
            var cmdText = @"DELETE FROM public.tradings;";
            await ExecuteCommand(connection, cmdText);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating tradings table: {ex.Message}");
            throw;
        }
    }

    private async Task ResetSessionsTable(NpgsqlConnection connection)
    {
        try
        {
            var cmdText = @"DELETE FROM public.sessions;";
            await ExecuteCommand(connection, cmdText);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating sessions table: {ex.Message}");
            throw;
        }
    }


    private async Task<bool> CheckDatabaseExists(string connectionString)
    {
        try
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var cmdText = @"SELECT 1 FROM pg_database WHERE datname='MTCG'";
                using (var command = new NpgsqlCommand(cmdText, connection))
                {
                    var result = await command.ExecuteScalarAsync();
                    return result != null;
                }
            }
        }
        catch (Exception)
        {
            return false;
        }
    }

    private async Task CreateDatabase(string connectionString)
    {
        try
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var cmdText = @"CREATE DATABASE ""MTCG""
                    WITH
                    OWNER = postgres
                    ENCODING = 'UTF8'
                    LC_COLLATE = 'German_Austria.1252'
                    LC_CTYPE = 'German_Austria.1252'
                    TABLESPACE = pg_default
                    CONNECTION LIMIT = -1
                    IS_TEMPLATE = False;"; await ExecuteCommand(connection, cmdText);
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    private async static Task<int> ExecuteCommand(NpgsqlConnection connection, string cmdText)
    {
        try
        {
            using var command = new NpgsqlCommand(cmdText, connection);
            return await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error executing command: {ex.Message}");
            throw;
        }
    }

    private async Task CreateTables(NpgsqlConnection connection)
    {
        try
        {
            await CreateCardsTable(connection);
            await CreatePackageCardsTable(connection);
            await CreatePackagesTable(connection);
            await CreateUsersTable(connection);
            await CreateUserStatsTable(connection);
            await CreateUserStatsView(connection);
            await CreateUserCardsTable(connection);
            await CreateTradingsTable(connection);
            await CreateSessionsTable(connection);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating tables: {ex.Message}");
            throw;
        }
    }

    private async Task CreateCardsTable(NpgsqlConnection connection)
    {
        var cmdText = @"CREATE TABLE IF NOT EXISTS public.cards
                    (
                        name character varying(50) NOT NULL,
                        damage double precision NOT NULL,
                        element character varying(20) NOT NULL,
                        type character varying(20) NOT NULL,
                        id character varying NOT NULL,
                        CONSTRAINT cards_pkey PRIMARY KEY (id)
                    )
                    TABLESPACE pg_default;";
        await ExecuteCommand(connection, cmdText);
    }

    private async Task CreatePackageCardsTable(NpgsqlConnection connection)
    {
        try
        {
            var cmdText = @"CREATE TABLE IF NOT EXISTS public.package_cards
                            (
                                id integer NOT NULL DEFAULT nextval('package_cards_id_seq'::regclass),
                                packageid integer,
                                cardid character varying(50),
                                CONSTRAINT package_cards_pkey PRIMARY KEY (id),
                                CONSTRAINT fk_packageid FOREIGN KEY (packageid)
                                    REFERENCES public.packages (id) MATCH SIMPLE
                                    ON UPDATE NO ACTION
                                    ON DELETE CASCADE,
                                CONSTRAINT package_cards_cardid_fkey FOREIGN KEY (cardid)
                                    REFERENCES public.cards (id) MATCH SIMPLE
                                    ON UPDATE NO ACTION
                                    ON DELETE NO ACTION
                            )
                            TABLESPACE pg_default;";
            await ExecuteCommand(connection, cmdText);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating package_cards table: {ex.Message}");
            throw;
        }
    }

    private async Task CreatePackagesTable(NpgsqlConnection connection)
    {
        try
        {
            var cmdText = @"CREATE TABLE IF NOT EXISTS public.packages
                            (
                                id integer NOT NULL DEFAULT nextval('packages_id_seq'::regclass),
                                packagename character varying(255) NOT NULL,
                                packagedescription text,
                                packagecost integer,
                                CONSTRAINT packages_pkey PRIMARY KEY (id)
                            )
                            TABLESPACE pg_default;";
            await ExecuteCommand(connection, cmdText);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating packages table: {ex.Message}");
            throw;
        }
    }

    private async Task CreateUsersTable(NpgsqlConnection connection)
    {
        try
        {
            var cmdText = @"CREATE TABLE IF NOT EXISTS public.users
                            (
                                id integer NOT NULL DEFAULT nextval('users_user_id_seq'::regclass),
                                username character varying(50) NOT NULL,
                                password_hash character varying(255) NOT NULL,
                                coins integer DEFAULT 20,
                                elo integer DEFAULT 100,
                                name character varying(255),
                                bio text,
                                image text,
                                CONSTRAINT users_pkey PRIMARY KEY (id),
                                CONSTRAINT users_username_key UNIQUE (username)
                            )
                            TABLESPACE pg_default;";
            await ExecuteCommand(connection, cmdText);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating users table: {ex.Message}");
            throw;
        }
    }

    private async Task CreateUserStatsTable(NpgsqlConnection connection)
    {
        try
        {
            var cmdText = @"CREATE TABLE IF NOT EXISTS public.userstats
                            (
                                userid integer NOT NULL,
                                wins integer DEFAULT 0,
                                losses integer DEFAULT 0,
                                CONSTRAINT pk_userstats PRIMARY KEY (userid),
                                CONSTRAINT fk_user FOREIGN KEY (userid)
                                    REFERENCES public.users (id) MATCH SIMPLE
                                    ON UPDATE NO ACTION
                                    ON DELETE CASCADE
                            )
                            TABLESPACE pg_default;";
            await ExecuteCommand(connection, cmdText);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating userstats table: {ex.Message}");
            throw;
        }
    }

    private async Task CreateUserStatsView(NpgsqlConnection connection)
    {
        try
        {
            var cmdText = @"
            CREATE OR REPLACE VIEW public.user_statsview
            AS
            SELECT u.id AS userid,
                u.username,
                u.name,
                u.elo,
                us.wins,
                us.losses,
                CASE
                    WHEN us.wins + us.losses = 0 THEN 0  -- To handle the case when there are no games played.
                    ELSE us.wins::double precision / (us.wins + us.losses)::double precision
                END AS winratio
            FROM users u
            JOIN userstats us ON u.id = us.userid;";
            await ExecuteCommand(connection, cmdText);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating user_statsview: {ex.Message}");
            throw;
        }
    }


    private async Task CreateUserCardsTable(NpgsqlConnection connection)
    {
        try
        {
            var cmdText = @"CREATE TABLE IF NOT EXISTS public.user_cards
                            (
                                userid integer REFERENCES public.users(id),
                                cardid character varying REFERENCES public.cards(id),
                                indeck boolean,
                                id integer NOT NULL DEFAULT nextval('user_cards_id_seq'::regclass),
                                lockedintrade boolean,
                                CONSTRAINT user_cards_pkey PRIMARY KEY(id)
                            )
                            TABLESPACE pg_default; ";
            await ExecuteCommand(connection, cmdText);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating user_cards table: {ex.Message}");
            throw;
        }
    }

    private async Task CreateTradingsTable(NpgsqlConnection connection)
    {
        try
        {
            var cmdText = @"CREATE TABLE IF NOT EXISTS public.tradings
                            (
                                id character varying(255) NOT NULL,
                                cardtotrade character varying(255) NOT NULL,
                                type character varying(255) NOT NULL,
                                minimumdamage double precision NOT NULL,
                                userid integer,
                                CONSTRAINT tradings_pkey PRIMARY KEY (id),
                                CONSTRAINT tradings_cardtotrade_fkey FOREIGN KEY (cardtotrade)
                                    REFERENCES public.cards (id) MATCH SIMPLE
                                    ON UPDATE NO ACTION
                                    ON DELETE NO ACTION,
                                CONSTRAINT tradings_userid_fkey FOREIGN KEY (userid)
                                    REFERENCES public.users (id) MATCH SIMPLE
                                    ON UPDATE NO ACTION
                                    ON DELETE NO ACTION
                            )
                            TABLESPACE pg_default;";
            await ExecuteCommand(connection, cmdText);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating tradings table: {ex.Message}");
            throw;
        }
    }

    private async Task CreateSessionsTable(NpgsqlConnection connection)
    {
        try
        {
            var cmdText = @"CREATE TABLE IF NOT EXISTS public.sessions
                            (
                                id integer NOT NULL DEFAULT nextval('sessions_id_seq'::regclass),
                                sessionid uuid NOT NULL,
                                starttime timestamp without time zone NOT NULL,
                                endtime timestamp without time zone,
                                userid integer,
                                CONSTRAINT sessions_pkey PRIMARY KEY (id),
                                CONSTRAINT fk_user FOREIGN KEY (userid)
                                    REFERENCES public.users (id) MATCH SIMPLE
                                    ON UPDATE NO ACTION
                                    ON DELETE CASCADE
                            )
                            TABLESPACE pg_default;";
            await ExecuteCommand(connection, cmdText);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating sessions table: {ex.Message}");
            throw;
        }
    }


}
