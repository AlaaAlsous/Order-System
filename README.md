# OrderSystem

![C#](https://img.shields.io/badge/C%23-239120?logo=csharp&logoColor=white)
![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)
![SQLite](https://img.shields.io/badge/SQLite-3-003B57?logo=sqlite&logoColor=white)
![Dapper](https://img.shields.io/badge/Dapper-ORM-7A5C61)
![Type](https://img.shields.io/badge/Type-Console%20App-darkcyan)
![License](https://img.shields.io/badge/License-MIT-green)

## Beskrivning

OrderSystem är en .NET-konsolapplikation som fungerar på alla plattformar som hanterar kunder, adresser, produkter, ordrar och orderrader via en interaktiv meny i terminalen. Data lagras lokalt i en SQLite-databas (`order_system.sqlite`) och alla operationer sker via Dapper för snabba och tydliga SQL-anrop.

Applikationen är uppdelad i modulära klasser (`Customer`, `Address`, `Product`, `Order`, `OrderItem`) som kapslar in respektive funktion. En hjälparklass (`ConsoleHelper`) sköter UI-rendering i konsolen: centrerad text, tabellutskrifter, färgteman samt en inmatningsfunktion som stödjer avbryt med `ESC`.

Databasen initieras automatiskt vid start, med tabeller, constraints (CHECK/FOREIGN KEY) samt en vy (`order_overview`) som sammanställer orderdata (kund, produkter, rader och totalsummor) för smidig visning.

### Krav och förutsättningar

- .NET SDK 10 (Target Framework: `net10.0`)
- SQLite (ingår via `Microsoft.Data.Sqlite`, ingen separat server krävs)
- Visual Studio eller VS Code med C#-stöd

---

## Projektstruktur

```text
OrderSystem/
├─ OrderSystem.slnx
├─ OrderSystem.csproj
├─ Program.cs
├─ ConsoleHelper.cs
├─ Customer.cs
├─ Address.cs
├─ Product.cs
├─ Order.cs
├─ OrderItem.cs
├─ bin/   (byggutdata)
└─ obj/   (mellanbuild)
```

- `Program.cs`: Startpunkt; initierar SQLite, skapar tabeller/vy, och kör huvudmenyn.
- `ConsoleHelper.cs`: UI-verktyg för centrerad text, färger, tabellutskrift och ESC-avbryt.
- `Customer.cs`: Skapa/visa/radera kunder; e-postvalidering och unicitetskontroll.
- `Address.cs`: Lägg till/visa adresser per kund; typkontroll (Delivery/Billing) och en per typ.
- `Product.cs`: Skapa/visa/radera produkter; namnunikhet, pris och lagersaldo.
- `Order.cs`: Skapa/visa/radera ordrar; statusuppdatering (Created/Paid/Delivered).
- `OrderItem.cs`: Skapa/visa/radera orderrader; koppling till produkt eller fristående beskrivning, lageruppdatering.

---

## English Summary

OrderSystem is a cross-platform .NET console app for managing customers, addresses, products, orders, and order items. It uses SQLite with Dapper and provides an interactive terminal menu. The database is created on first run.

Run:

```bash
cd OrderSystem
dotnet run
```

Or build:

```bash
dotnet build OrderSystem/OrderSystem.csproj
```

---

## Huvudmeny och navigering

Huvudmenyn visas i konsolen med sektioner och val:

- CUSTOMER
  - Create Customer
  - Show Customers
  - Delete Customer
  - Add Address to Customer
  - Show Addresses
- PRODUCT
  - Create Product
  - Show Products
  - Delete Product
- ORDER
  - Create Order
  - Show Orders
  - Update Order Status
  - Delete Order
- ORDER ITEM
  - Create Order Item
  - Show Order Items
  - Delete Order Item
- EXIT

### Styrning

- Piltangenter `↑`/`↓`: flytta mellan menyval
- `Enter`: välj markerat alternativ
- Inmatningar i formulär kan avbrytas med `ESC` (återgår till huvudmenyn)


---

## Funktioner

- **Interaktiv konsolmeny**: Tydlig, färgsatt meny med centrering och tabellutskrifter.
- **SQLite-persistens**: Lokalt datalager; automatiskt schema-setup vid uppstart.
- **Dapper-queries**: Enkla och snabba SQL-anrop med tydlig parameterhantering.
- **Datavalidering**: Längdkontroller, formatkontroller (e-post, datum), och affärsregler.
- **Orderstatus-flöde**: Uppdatera orderstatus mellan `Created`, `Paid`, `Delivered`.
- **Lagerhantering**: Orderrader som kopplas till produkt minskar lagersaldo automatiskt.
- **Flexibel orderrad**: Antingen koppling till produkt eller fristående `description` (med CHECK-regel).
- **Summeringar via vy**: `order_overview` ger totaler per order samt detaljer per rad.
- **Säkra raderingar**: Utnyttjar FOREIGN KEY-kedjor och CASCADE där relevant (t.ex. adresser och orderrader).

---

## Databasdesign

Applikationen skapar följande schema (förenklad beskrivning):

- **`customers`**: `Id` (PK), `name`, `email` (UNIQUE), `phone`
- **`addresses`**: `id` (PK), `customer_id` (FK → `customers.Id` ON DELETE CASCADE), `address_type` (CHECK: Delivery/Billing), `street`, `city`, `zip_code`, `country`
- **`products`**: `id` (PK), `name` (UNIQUE, `length(name) ≤ 30`), `unit_price`, `stock`
- **`orders`**: `id` (PK), `customer_id` (FK), `order_date` (UNIX-sekunder), `status` (CHECK: Created/Paid/Delivered)
- **`order_rows`**: `id` (PK), `order_id` (FK → `orders.id` ON DELETE CASCADE), `product_id` (FK → `products.id`, nullable), `description` (nullable, `length(description) ≤ 30`), `quantity`, `unit_price`, `CHECK (product_id IS NOT NULL OR description IS NOT NULL)`
- **Vy `order_overview`**: Joinar order, kund, rader och produktinfo; beräknar `totalprice = quantity * unit_price` per rad.

Dessutom aktiveras `PRAGMA foreign_keys = ON;` vid uppstart.

---

## Så här kör du programmet

```bash
cd OrderSystem
dotnet run
```

Bygga utan att köra:

```bash
dotnet build OrderSystem/OrderSystem.csproj
```

### Förväntat flöde vid start

- Menyn visas med sektionerna ovan.
- Välj `Create Customer` för att lägga till kund + en första adress.
- Lägg till produkter (`Create Product`).
- Skapa order (`Create Order`).
- Lägg till orderrader (`Create Order Item`), koppla ev. produkt; lagersaldo uppdateras.
- Visa översikter (`Show Customers/Products/Orders/Order Items`).
- Uppdatera status (`Update Order Status`).

---


## Teknisk miljö

- **Target Framework**: `net10.0`
- **Paket**:
  - `Dapper` (`2.1.66`)
  - `Microsoft.Data.Sqlite` (`10.0.2`)
- **Databasfil**: `order_system.sqlite` (skapas i aktuell arbetskatalog)

---


## Utvecklare

**Alaa Alsous**  

Språk: C#  
Plattform: .NET 10   
Verktyg: Visual Studio  