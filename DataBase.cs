using MySqlConnector;

namespace SinafProduction;

public class DataBase(MySqlDataSource web, MySqlDataSource pussInBot)
{
    public readonly MySqlDataSource web = web;
    public readonly MySqlDataSource pussInBot = pussInBot;
}