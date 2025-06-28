## 🚀 Getting Started

1. **Clone the repository**

   ```bash
   git clone https://github.com/neamat205/MinAccApp.git
   cd MinAccApp
   ```

2. **Set up the database**

   - Open SQL Server Management Studio
   - Create a new database (e.g., `MinAccDB`)
   - Run all `.sql` files from the `Database/` folder (tables first,then user defined types, then stored procedures)

3. **Configure `appsettings.json`**

   - Add Connection String As Your Hosted DB

   ```json
   "ConnectionStrings": {
     "DefaultConnection": ""
   }
   ```

4. **Run the application**
   - In Visual Studio: Press F5
   - Or CLI:
     ```bash
     dotnet restore
     dotnet run
     ```

---

## Screenshots

### ➕ Add Voucher

![Project Screenshot](images/add-voucher.png)

### 💵 Voucher List

![Project Screenshot](images/voucher-list.png)

### 🏠 Home Page

![Project Screenshot](images/home.png)

### 🔐 Login Page

![Project Screenshot](images/login.png)

### 📊 Chart of Accounts

![Project Screenshot](images/manage-coa.png)

### 🔐 Registration Page

![Project Screenshot](images/registration.png)

### 🧑‍💼 User Management

![Project Screenshot](images/user-management.png)
