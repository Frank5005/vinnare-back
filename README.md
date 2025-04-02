# 🧠 Back End Challenge
This README includes all the necessary information about the project:
 
 ---
 
## 🛠 Tools
- ASP.NET Core
- Entity Framework Core
- PostgreSQL
- PgAdmin and DBeaver
- AWS
- Docker
- GitHub Actions
- xUnit
- Swagger
- Postman

---

## 🚀 Methodology used
**Trunk Based Development**, because:
- ✅ More effective continuous integration.  
- 🚀 Fast and frequent delivery of functionality.  
- 🔄 Less complexity in branch management.  
- 🤖 Facilitates pipeline automation.

How we organized it to achieve all the requirements?
Using **Jira** → [Project Board](https://vinnare.atlassian.net/jira/software/projects/SCRUM/list?sortBy=duedate&direction=ASC&atlOrigin=eyJpIjoiNzMzOGFiNGE4YzViNDY0ZTg1MGRlZTMyNjFkZDRiZjQiLCJwIjoiaiJ9)

---

## 🗃️ Entity Relationship Diagram
![Final ER Diagram Back End Challenge](https://github.com/user-attachments/assets/79d672cb-227e-4e36-bf7b-a8f895e7edc0)


---

## 🧪 Unit tests
![image (5)](https://github.com/user-attachments/assets/07af065b-773c-4004-899d-008b66582efb)

---

## 🧰 Instructions to run the application
### 📋 Prerequisites
Before you start, make sure you have installed on your computer :
- .NET 6.0 SDK or higher
- PostgreSQL
- Entity Framework CLI (dotnet-ef)
- An editor such as Visual Studio or Visual Studio Code
### 🛠️ 1. Clone the repository
```bash
git clone https://github.com/dcanasp/vinnare.git
cd vinnare
```
### ⚙️ 2. Setting up the database connection
Edit the appsettings.json or appsettings.Development.json file with the connection string to your PostgreSQL database:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=name_database;Username=user;Password=password"
}
```
### 🧱 3. Create the database 
You can create the database manually from a tool like pgAdmin or from the console:
```bash
psql -U postgres
CREATE DATABASE database_name;
```
### 4. Apply migrations
```bash
dotnet ef database update
```
This will create all tables according to your existing models and migrations.
### ▶️ 5. Run the application
```bash
dotnet run
```
The API will be available (by default) in:
```console
https://localhost:5001
http://localhost:5000
```
You can test the endpoints with tools like Postman or directly from Swagger in:
```console
https://localhost:5001/swagger
```
