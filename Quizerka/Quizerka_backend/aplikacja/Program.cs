using aplikacja;
using MySqlConnector;
using Newtonsoft.Json;
using System.Xml.Linq;
using System;
using System.Security.Cryptography;


// using MySqlConnector - MySqlConnector biblioteka umożliwiającą połączenie z bazą danych MySQL
// using Newtonsoft.Json - biblioteka umożliwiająca serializację i deserializację obiektów do i z formatu JSON
// using System.Xml.Linq biblioteka umożliwiająca pracę z dokumentami XML (odczyt, tworzenie i modyfikowanie)
// using System pozwala na korzystanie z wielu podstawowych klas i interfejsów, takich jak np. klasy dotyczące daty i czasu, tablic, plików i wyjątków
// using System.Security.Cryptography - zawiera klasy i interfejsy do wykonywania operacji szyfrowania i hashowania danych

// kwerenda SQL do utworzenia tabeli
/*CREATE TABLE USER (
 id INT NOT NULL AUTO_INCREMENT,
 login VARCHAR(255) NOT NULL,
 password VARCHAR(255) NOT NULL,
 score INT,
 PRIMARY KEY (id)
);
*/
var connectionString = "Server=127.0.0.1;User ID=root;Password=;Port=3306;Database=world";
// zmienna przechowująca ciąg połączenia do bazy danych MySQL
//  linia zawiera informacje potrzebne do połączenia z bazą danych: adres serwera (127.0.0.1), nazwę użytkownika (root), hasło (tutaj bez hasła), port (3306) i nazwę bazy danych (world)

MySqlConnection connection = new MySqlConnection(connectionString);
//  tworzymy obiekt klasy MySqlConnection do połączenia się z bazą danych MySQL z użyciem zdefiniowanych wcześniej informacji w connectionString
 
var builder = WebApplication.CreateBuilder(args);
//  tworzymy obiekt konfigurujący połączenie z aplikacją webową (nasz frontend), przy użyciu metody CreateBuilder z klasy WebApplication, przyjumącą  argumenty args.


// poniżej konfigurujemy ORS (Cross-Origin Resource Sharing), który pozwala na udostępnianie zasobów pomiędzy frontendem i backendem
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "ABC",
    //  dodajemy CORS o nazwie "ABC"
                      policy =>
                      {
                        //    poniżej ustawiamy origin, któremu aplikacja webowa ma udostępniać zasoby - w tym przypadku jest to "http://localhost:3000"
                          policy.WithOrigins("http://localhost:3000");
                      });
});

// tworzymy aplikację
var app = builder.Build();



// Korzystamy z metody UseCors na obiekcie app
app.UseCors(x => x.AllowAnyHeader()
      .AllowAnyMethod()
      .WithOrigins("http://localhost:3000"));
// konfigurujemy middleware CORS, który pozwala na korzystanie z dowolnych nagłówków i metod podczas wykonywania żądań HTTP do frontendu, który postawiony jest na http://localhost:3000




List<Quiz> items; //jest to zmienna, która deklaruje listę obiektów Quiz
using (StreamReader r = new StreamReader("quiz.json"))
{
    // tworzymy instancję klasy StreamReader, która będzie używana do wczytywania danych z pliku quiz.json
    // plik jest przekazywany jako argument do konstruktora klasy StreamReader
    // instrukcja using zapewnia, że plik jest "zamknięty" i poprawnie są zwalniane zasoby, gdy zostanie wykonana ostatnia linia bloku kodu 
    string json = r.ReadToEnd(); // instrukcja, która odczytuje całą zawartość pliku "quiz.json" i przechowuje ją w zmiennej json
    items = JsonConvert.DeserializeObject<List<Quiz>>(json);
    // instrukcja, deserializująca zawartość zmiennej "json" do listy obiektów Quiz za pomocą metody DeserializeObject 
    
}

 
//  konfiguruujemy metodę HTTP typu GET i ścieżkę URL /get-quiz, obsługiwaną przez aplikację
app.MapGet("/get-quiz", () =>
{
    // generujemy liczbę pseudolosową
    Random random = new Random();
    // tworzymy listę obiektów Quiz
    List<Quiz> quizy = new List<Quiz>();

    for (int i = 0; i < 5; i++)
    {
        int index = random.Next(0, items.Count());
        // generujemy 5 losowych pytań z listy items i dodajemy do listy quizy
        Quiz element = items[index];
        quizy.Add(element);
    } 
    
    // zwracamy pytania w odpowiedzi na zapytanie HTTP
    return quizy;
});

app.MapPost("/register", async (User user) =>
// to samo jak wcześniej tylko dla ścieżki /register, W tym przypadku jest metoda POST
// funkcja oczekuje obiektu User jako argument, który zawiera informacje o użytkowniku
{ 
    Console.WriteLine(user.Login);
    Console.WriteLine(user.Password);
    // Kodowanie hasła
    using (MD5 md5 = MD5.Create())
    //  tworzymy obiekt klasy MD5 do zahashowania
    {
        // konwertujemy hasło do tablicy bajtów
        byte[] passwordBytes = System.Text.Encoding.ASCII.GetBytes(user.Password);
        // generujemy has za pomocą metody comptuehas
        byte[] encodedPassword = md5.ComputeHash(passwordBytes);
        // konwertujemy hash na string i usuwamy myślniki. Ponadto, zmieniamy ciąg znaków na małe litery
        string encodedPasswordString = BitConverter.ToString(encodedPassword).Replace("-", "").ToLower();
        // przypisujemy hasło do user.Password
        user.Password = encodedPasswordString;
    }

    // query to zmienna zawierająca kwerendę SQL
    string query = "INSERT INTO user (login, password, score) VALUES (@login, @password, 0)";

    // tworzymy obiekt cmd, aby połączyć się z bazą danych i przekazać query
    MySqlCommand cmd = new MySqlCommand(query, connection);
    // dodajemy wartości
    cmd.Parameters.AddWithValue("@login", user.Login);
    cmd.Parameters.AddWithValue("@password", user.Password); 

    // otwieramy połączenie z bazą danyc
    connection.Open();
    // wykonujemy zapytanie
    cmd.ExecuteNonQuery();
    // pobieramy id z ostatniego rekordu do bazy danychj
    long id = cmd.LastInsertedId;
    // zamykamy połączenie z bazą danych
    connection.Close();
    // zwracamy obiekt response w odpowiedzi na żadanie HTTP
    var response = new { msgType = "OK"};
    return response;
});

// odbieramy żdanie HTTP typu POST dla ścieżki /set-score
app.MapPost("/set-score", async (Score score) =>
{
    // oczekujemy obiektu Score jako argumeny, ktory zawiera informacje o użytkowniu i jego wyniku
    // query to zmienna zawierająca kwerendę SQL
    //  aktualizujemy rekord w tabeli user z wartościami score i user_id
    string query = "UPDATE `user` SET `score` = (@score) WHERE `id` = (@user_id)"; 

    // twrzymy obiekt z kwerendą i połączeniem do bazy danych
    MySqlCommand cmd = new MySqlCommand(query, connection);
    // dodajemy wartości do zapytania SQL, odpowiadających parametrom score i user_id w przesłanym w obiekcie Score
    cmd.Parameters.AddWithValue("@score", score.score);
    cmd.Parameters.AddWithValue("@user_id", score.user_id);

    // otwieramy połączenie z bazą danych
    connection.Open();
    // wykonujemy zapytanie SQL
    cmd.ExecuteNonQuery(); 
    // zamykamy połączenie z bazą danych
    connection.Close();
    // tworzymy obiekt z informacją, że operacja została z sukcesem zrealizowana
    var response = new { msgType = "Succes" };
    // w odpowiedzi na żądanie zwracamy obiekt
    return response;
});


app.MapPost("/get-score", async (UserInfo user) =>
{
    // tak samo jak wcześniej tylko że pobieramy tutaj score dla usera
    string query = "SELECT `score` from `user` where id = (@user_id);";
    // twrzymy obiekt z kwerendą i połączeniem do bazy danych
    MySqlCommand cmd = new MySqlCommand(query, connection); 
    // dodajemy wartość
    cmd.Parameters.AddWithValue("@user_id", user.user_id);
    // otwieramy połączenie z bazą danych

    connection.Open();

    // wykonujemy zapytanie SQL i za pomocą ExecuteScalar zwracany jest obiket, który mozże być rzutowany na dowolny typ np. in/string
    var score = cmd.ExecuteScalar();
    // zamykamy połączenie z bazą danych
    connection.Close();

     
    Console.WriteLine(score);
    // w odpowiedzi na żądanie zwracamy obiekt z wynikiem
    var response = new { score=score };
    return response;
});


// endpoint dotyczący logowania
// metoda POST
// /login
app.MapPost("/login", async (User user) =>
{

    string query = "SELECT `id` from `user` where (login = (@user_id) and password = (@uss));";

    MySqlCommand cmd = new MySqlCommand(query, connection);
    using (MD5 md5 = MD5.Create())
//  tworzymy obiekt klasy MD5 do zahashowania
    {
        // konwertujemy hasło do tablicy bajtów
        byte[] passwordBytes = System.Text.Encoding.ASCII.GetBytes(user.Password);
        // generujemy has za pomocą metody comptuehas
        byte[] encodedPassword = md5.ComputeHash(passwordBytes);
        // konwertujemy hash na string i usuwamy myślniki. Ponadto, zmieniamy ciąg znaków na małe litery
        string encodedPasswordString = BitConverter.ToString(encodedPassword).Replace("-", "").ToLower();
        // przypisujemy hasło do user.Password
        user.Password = encodedPasswordString;
    }

    // wykonujemy zapytanie SQL i sprawdzamy czy istnieje taki user
    cmd.Parameters.AddWithValue("@user_id", user.Login);
    cmd.Parameters.AddWithValue("@uss", user.Password);
    // otwieramy połączenie
    connection.Open();
    // jeśli user istnieje, otrzymujemy jego id
    var id = cmd.ExecuteScalar();
    // zamykamy połączenie
    connection.Close();


    Console.WriteLine(id);

    // w odpowiedzi na żadanie HTTP wysylamy id w obiekcie
    var response = new { userID = id };
    return response;
});

// uruchamiamy aplikację
app.Run();
 
