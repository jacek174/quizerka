using aplikacja;
using MySqlConnector;
using Newtonsoft.Json;
using System.Xml.Linq;

/*CREATE TABLE USER (
 id INT NOT NULL AUTO_INCREMENT,
 login VARCHAR(255) NOT NULL,
 password VARCHAR(255) NOT NULL,
 score INT,
 PRIMARY KEY (id)
);
*/
var connectionString = "Server=127.0.0.1;User ID=root;Password=;Port=3306;Database=world";
MySqlConnection connection = new MySqlConnection(connectionString);

 
var builder = WebApplication.CreateBuilder(args);
 


builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "ABC",
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:3000");
                      });
});

var app = builder.Build();



app.UseCors(x => x.AllowAnyHeader()
      .AllowAnyMethod()
      .WithOrigins("http://localhost:3000"));





List<Quiz> items;
using (StreamReader r = new StreamReader("quiz.json"))
{
    string json = r.ReadToEnd();
    items = JsonConvert.DeserializeObject<List<Quiz>>(json);
 
}

 

app.MapGet("/get-quiz", () =>
{
    Random random = new Random();

    List<Quiz> quizy = new List<Quiz>();

    for (int i = 0; i < 5; i++)
    {
        int index = random.Next(0, items.Count());

        Quiz element = items[index];
        quizy.Add(element);
    } 
    
    return quizy;
});

app.MapPost("/register", async (User user) =>
{ 
    Console.WriteLine(user.Login);
    Console.WriteLine(user.Password);

     
    string query = "INSERT INTO user (login, password, score) VALUES (@login, @password, 0)";

    MySqlCommand cmd = new MySqlCommand(query, connection);
    cmd.Parameters.AddWithValue("@login", user.Login);
    cmd.Parameters.AddWithValue("@password", user.Password); 

    connection.Open();
    cmd.ExecuteNonQuery();
    long id = cmd.LastInsertedId;
    connection.Close();
    var response = new { msgType = "OK"};
    return response;
});

app.MapPost("/set-score", async (Score score) =>
{

    string query = "UPDATE `user` SET `score` = (@score) WHERE `id` = (@user_id)"; 

    MySqlCommand cmd = new MySqlCommand(query, connection);
    cmd.Parameters.AddWithValue("@score", score.score);
    cmd.Parameters.AddWithValue("@user_id", score.user_id);

    connection.Open();
    cmd.ExecuteNonQuery(); 
    connection.Close();

    var response = new { msgType = "Succes" };
    return response;
});


app.MapPost("/get-score", async (UserInfo user) =>
{
    string query = "SELECT `score` from `user` where id = (@user_id);";

    MySqlCommand cmd = new MySqlCommand(query, connection); 
    cmd.Parameters.AddWithValue("@user_id", user.user_id);
    connection.Open();
    var score = cmd.ExecuteScalar();
    connection.Close();

     
    Console.WriteLine(score);

    var response = new { score=score };
    return response;
});



app.MapPost("/login", async (User user) =>
{
    string query = "SELECT `id` from `user` where (login = (@user_id) and password = (@uss));";

    MySqlCommand cmd = new MySqlCommand(query, connection);
    cmd.Parameters.AddWithValue("@user_id", user.Login);
    cmd.Parameters.AddWithValue("@uss", user.Password);
    connection.Open();
    var id = cmd.ExecuteScalar();

    connection.Close();


    Console.WriteLine(id);

    var response = new { userID = id };
    return response;
});


app.Run();
 