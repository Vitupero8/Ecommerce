# E-Commerce Microservices

## Opis projekta

Ovaj projekat predstavlja backend e-commerce sistema implementiranog korišćenjem mikroservisne arhitekture.

Sistem je podijeljen na više servisa, gdje svaki servis ima jasno definisanu odgovornost. Servisi međusobno komuniciraju putem HTTP zahtjeva i REST API-ja, bez direktnog pristupa bazama podataka drugih servisa.

Projekat obuhvata upravljanje korisnicima, proizvodima, korpama, narudžbinama, plaćanjima i pošiljkama.

Za autentifikaciju korisnika koristi se JWT (JSON Web Token), dok se PayPal plaćanje testira kroz PayPal Sandbox okruženje.

## Arhitektura

Projekat se sastoji od sljedećih mikroservisa:

* **CustomerService** – registracija, prijava i upravljanje korisnicima.
* **ProductService** – upravljanje proizvodima, cijenama i stanjem proizvoda.
* **CartService** – upravljanje korpama, dodavanje proizvoda, promjena količine, uklanjanje proizvoda i checkout.
* **PaymentService** – kreiranje narudžbina i obrada različitih načina plaćanja.
* **ShipmentService** – kreiranje i praćenje pošiljki nakon uspješnog plaćanja.

Osnovni tok sistema je:

**Customer → Product → Cart → Payment → Shipment**

Primjer toka kupovine:

1. Korisnik se registruje i prijavljuje.
2. Korisnik kreira korpu.
3. Proizvod se dodaje u korpu uz provjeru dostupnog stanja i trenutne cijene.
4. Prilikom checkout-a ponovo se provjeravaju cijena i stanje proizvoda.
5. CartService šalje podatke o narudžbini PaymentService-u.
6. PaymentService kreira narudžbinu.
7. Korisnik bira način plaćanja.
8. Nakon uspješnog plaćanja narudžbina dobija status `Paid`.
9. ShipmentService provjerava da je narudžbina plaćena i kreira pošiljku.
10. Pošiljka prolazi kroz statuse `Order Received`, `Shipped`, `In Transit`, `Out for Delivery` i `Delivered`.

## Korišćene tehnologije

* **C#** – programski jezik korišćen za razvoj backend servisa.
* **.NET 10** – platforma na kojoj su razvijeni mikroservisi.
* **ASP.NET Core Web API** – razvoj REST API-ja i HTTP endpoint-a.
* **Entity Framework Core** – pristup i rad sa bazama podataka.
* **MySQL** – relaciona baza podataka.
* **JWT (JSON Web Token)** – autentifikacija i autorizacija korisnika.
* **PayPal Sandbox** – simulacija i testiranje PayPal plaćanja.
* **Postman** – ručno testiranje REST API endpoint-a.
* **xUnit** – automatsko testiranje.
* **Moq** – mockovanje zavisnosti u testovima.
* **Git / GitHub** – verzionisanje i čuvanje izvornog koda.

## Struktura projekta

```text
CustomerService/
│
├── Database/
│   └── Ecommerce.sql
│
├── CustomerService/
├── CustomerService.Tests/
│
├── ProductService/
├── ProductService.Tests/
│
├── CartService/
├── CartService.Tests/
│
├── PaymentService/
├── PaymentService.Tests/
│
├── ShipmentService/
├── ShipmentService.Tests/
│
├── CustomerService.slnx
├── README.md
└── .gitignore
```

### CustomerService

Zadužen je za upravljanje korisnicima.

Omogućava:

* registraciju korisnika;
* prijavu korisnika;
* JWT autentifikaciju;
* pregled podataka korisnika;
* izmjenu podataka korisnika;
* brisanje korisnika.

Lozinke se čuvaju kao hash vrijednosti, a ne kao običan tekst.

### ProductService

Zadužen je za upravljanje proizvodima.

Čuva podatke o:

* nazivu proizvoda;
* tipu proizvoda;
* cijeni;
* količini proizvoda na stanju.

Omogućava kreiranje, pregled, izmjenu i brisanje proizvoda.

### CartService

Zadužen je za upravljanje korpama korisnika.

Omogućava:

* kreiranje korpe;
* dodavanje proizvoda;
* provjeru dostupnog stanja proizvoda;
* provjeru trenutne cijene proizvoda;
* promjenu količine;
* uklanjanje proizvoda;
* preračunavanje ukupne cijene;
* checkout korpe.

CartService komunicira sa ProductService-om putem HTTP zahtjeva kako bi dobio podatke o proizvodima.

Prilikom checkout-a CartService ponovo provjerava cijenu i dostupno stanje proizvoda prije kreiranja narudžbine u PaymentService-u.

### PaymentService

Zadužen je za upravljanje narudžbinama i plaćanjem.

Podržani načini plaćanja su:

* Card;
* PayPal;
* Apple Pay;
* Bank Transfer.

Card i Apple Pay predstavljaju simulaciju procesa plaćanja za potrebe testiranja.

PayPal je povezan sa PayPal Sandbox okruženjem.

PaymentService prati stanje narudžbine i plaćanja i nakon uspješnog plaćanja postavlja status narudžbine na `Paid`.

Servis takođe podržava čuvanje načina plaćanja korisnika i određivanje podrazumijevanog načina plaćanja.

### ShipmentService

Zadužen je za upravljanje pošiljkama nakon uspješnog plaćanja.

Prilikom kreiranja pošiljke provjerava da li narudžbina postoji, da li pripada prijavljenom korisniku i da li je plaćena.

Nakon toga preuzima podatke korisnika od CustomerService-a i čuva ih kao snapshot konkretne pošiljke.

Na ovaj način kasnija promjena adrese korisnika ne mijenja adresu već kreirane pošiljke.

Pošiljka može imati sljedeće statuse:

```text
Order Received
Shipped
In Transit
Out for Delivery
Delivered
```

## Baze podataka

Servisi ne pristupaju direktno tabelama drugih servisa.

`CustomerService` i `ProductService` trenutno koriste istu fizičku MySQL bazu `Ecommerce`, ali svaki servis pristupa samo tabelama koje mu pripadaju.

`CartService`, `PaymentService` i `ShipmentService` koriste odvojene fizičke baze podataka.

Podaci između servisa razmjenjuju se putem HTTP zahtjeva i REST API-ja.

### CustomerService

Baza:

```text
Ecommerce
```

Tabela:

```text
Customers
```

Tabela sadrži podatke o korisnicima, uključujući ime, prezime, telefon, email, adresu i hash lozinke.

### ProductService

Baza:

```text
Ecommerce
```

Tabela:

```text
Product
```

Tabela sadrži podatke o proizvodima, njihovoj cijeni i količini na stanju.

### CartService

Baza:

```text
CartService
```

Tabele:

```text
Carts
CartItems
```

`Carts` čuva podatke o korpama korisnika i njihovoj ukupnoj cijeni.

`CartItems` čuva proizvode koji se nalaze u pojedinačnoj korpi, njihovu količinu i cijenu u trenutku dodavanja u korpu.

### PaymentService

Baza:

```text
PaymentService
```

Tabele:

```text
Orders
OrderItem
Payments
PaymentMethods
```

`Orders` čuva podatke o narudžbinama.

`OrderItem` čuva proizvode koji pripadaju pojedinačnoj narudžbini.

`Payments` čuva podatke o pokušajima plaćanja i njihovom statusu.

`PaymentMethods` čuva sačuvane načine plaćanja korisnika i informaciju o podrazumijevanom načinu plaćanja.

### ShipmentService

Baza:

```text
ShipmentService
```

Tabela:

```text
Shipments
```

Tabela čuva podatke o pošiljkama i njihovom trenutnom statusu.

Podaci korisnika koji su potrebni za dostavu čuvaju se kao dio konkretne pošiljke.

### Komunikacija između servisa

Servisi međusobno komuniciraju putem HTTP zahtjeva.

Na primjer, CartService ne pristupa direktno tabeli `Product`.

Umjesto toga, CartService šalje zahtjev ProductService-u i od njega dobija podatke o proizvodu.

Slično tome:

* CartService šalje podatke o checkout-u PaymentService-u;
* ShipmentService provjerava narudžbinu preko PaymentService-a;
* ShipmentService preuzima podatke korisnika preko CustomerService-a.

Ovakav pristup omogućava da servisi budu međusobno nezavisni na nivou pristupa podacima.

## Pokretanje projekta

### Preduslovi

Prije pokretanja projekta potrebno je imati instalirano:

* .NET 10 SDK;
* MySQL Server;
* Visual Studio ili drugi IDE koji podržava .NET 10;
* Postman za ručno testiranje API-ja.

### 1. Preuzimanje projekta

Klonirati repository:

```bash
git clone https://github.com/Vitupero8/Ecommerce.git
```

Nakon toga otvoriti:

```text
CustomerService.slnx
```

u Visual Studio-u.

### 2. Kreiranje baza podataka

U folderu `Database` nalazi se SQL skripta:

```text
Database/Ecommerce.sql
```

Skripta kreira sve potrebne baze i tabele za projekat:

```text
Ecommerce
CartService
PaymentService
ShipmentService
```

Potrebno je otvoriti `Ecommerce.sql` u MySQL Workbench-u ili drugom MySQL alatu i izvršiti cijelu skriptu.

Skripta takođe dodaje nekoliko početnih proizvoda u `Product` tabelu kako bi CartService mogao odmah da se testira.

### 3. Konfiguracija MySQL-a

Servisi koriste lokalni MySQL Server.

Connection string treba prilagoditi lokalnoj MySQL instalaciji.

Primjer:

```text
Server=localhost;
Port=3306;
Database=CartService;
User=root;
Password=YOUR_MYSQL_PASSWORD;
```

Naziv baze zavisi od servisa:

```text
CustomerService → Ecommerce
ProductService  → Ecommerce
CartService     → CartService
PaymentService  → PaymentService
ShipmentService → ShipmentService
```

Ako je MySQL korisnik `root` zaštićen lozinkom, potrebno je podesiti connection string svakog servisa tako da koristi odgovarajuću lokalnu lozinku.

### 4. JWT konfiguracija

CustomerService koristi JWT za autentifikaciju.

Nakon uspješne prijave korisnik dobija JWT token koji se koristi za pristup zaštićenim endpoint-ima.

CartService, PaymentService i ShipmentService koriste isti JWT secret kako bi mogli validirati tokene koje izdaje CustomerService.

JWT secret treba biti podešen u lokalnoj konfiguraciji servisa.

### 5. PayPal Sandbox konfiguracija

PaymentService koristi PayPal Sandbox za testiranje PayPal plaćanja.

Za PayPal testiranje potrebni su:

* PayPal Client ID;
* PayPal Client Secret;
* PayPal Sandbox nalog.

PayPal podaci se podešavaju lokalno kroz konfiguraciju.

PayPal Sandbox omogućava simulaciju plaćanja bez korišćenja pravog novca.

### 6. HTTPS sertifikat

Servisi koriste HTTPS tokom lokalnog razvoja.

Ukoliko .NET prijavi problem sa development HTTPS sertifikatom, može se koristiti:

```bash
dotnet dev-certs https --trust
```

### 7. Pokretanje servisa

Potrebno je pokrenuti svih pet servisa:

```text
CustomerService
ProductService
CartService
PaymentService
ShipmentService
```

Servisi koriste sljedeće HTTPS portove:

```text
CustomerService → https://localhost:7252
ProductService  → https://localhost:7138
CartService     → https://localhost:7211
PaymentService  → https://localhost:7112
ShipmentService → https://localhost:7022
```

Svi servisi moraju biti pokrenuti istovremeno jer međusobno komuniciraju putem HTTP zahtjeva.

## API testiranje pomoću Postman-a

REST API endpoint-i su ručno testirani pomoću Postman-a.

Testiranje treba pratiti sljedeći redosljed:

```text
Customer
   ↓
Product
   ↓
Cart
   ↓
Payment
   ↓
Shipment
```

### 1. CustomerService

Registracija:

```http
POST https://localhost:7252/api/Customer/register
```

Primjer:

```json
{
    "name": "Test",
    "surname": "User",
    "phone": "067123456",
    "email": "test@example.com",
    "address": "Podgorica, Montenegro",
    "password": "Test123!"
}
```

Prijava:

```http
POST https://localhost:7252/api/Customer/login
```

Primjer:

```json
{
    "email": "test@example.com",
    "password": "Test123!"
}
```

Login vraća JWT token.

Token je potrebno koristiti kao:

```text
Authorization → Bearer Token
```

za endpoint-e koji zahtijevaju autentifikaciju.

### 2. ProductService

Pregled proizvoda:

```http
GET https://localhost:7138/api/Product
```

Kreiranje proizvoda:

```http
POST https://localhost:7138/api/Product
```

Primjer:

```json
{
    "productName": "Test Product",
    "productType": "Electronics",
    "productPrice": 49.99,
    "productStockQuantity": 20
}
```

Nakon kreiranja proizvoda sačuvati njegov `ProductID`.

### 3. CartService

Kreiranje korpe:

```http
POST https://localhost:7211/api/Cart
```

Primjer:

```json
{
    "totalPrice": 0
}
```

Nakon kreiranja sačuvati `CartID`.

Dodavanje proizvoda:

```http
POST https://localhost:7211/api/Cart/product
```

Primjer:

```json
{
    "cartId": 1,
    "productId": 1,
    "quantity": 2
}
```

Promjena količine:

```http
PUT https://localhost:7211/api/Cart/{cartItemId}
```

Primjer:

```json
{
    "quantity": 3
}
```

Brisanje proizvoda:

```http
DELETE https://localhost:7211/api/Cart/{cartItemId}
```

Checkout:

```http
POST https://localhost:7211/api/Cart/{cartId}
```

Checkout ponovo provjerava cijene i dostupno stanje proizvoda prije kreiranja narudžbine.

### 4. PaymentService

Nakon checkout-a kreira se narudžbina u PaymentService-u.

Pregled narudžbine:

```http
GET https://localhost:7112/api/Order/{orderId}
```

#### Card

```http
POST https://localhost:7112/api/Order/{orderId}/card-payment
```

Primjer uspješnog testnog zahtjeva:

```json
{
    "cardNumber": "4111111111111111",
    "expiryDate": "12/30",
    "cvv": "123"
}
```

#### Apple Pay

```http
POST https://localhost:7112/api/Order/{orderId}/apple-pay
```

Primjer:

```json
{
    "token": "APPLE-PAY-12345"
}
```

#### Bank Transfer

Pokretanje transfera:

```http
POST https://localhost:7112/api/Order/{orderId}/bank-transfer
```

Nakon toga se transfer potvrđuje pomoću `paymentId` vrijednosti:

```http
POST https://localhost:7112/api/Order/bank-transfer/{paymentId}/confirm
```

#### PayPal

Pokretanje PayPal plaćanja:

```http
POST https://localhost:7112/api/Order/{orderId}/payment
```

PaymentService vraća podatke potrebne za PayPal Sandbox plaćanje.

Nakon odobravanja plaćanja u PayPal Sandbox okruženju, PayPal transakcija se završava i narudžbina dobija status `Paid`.

### 5. ShipmentService

Pošiljka se može kreirati tek nakon uspješnog plaćanja narudžbine.

Kreiranje:

```http
POST https://localhost:7022/api/Shipment
```

Primjer:

```json
{
    "orderID": 1
}
```

ShipmentService provjerava da li narudžbina postoji i da li je plaćena.

Pregled svih pošiljki:

```http
GET https://localhost:7022/api/Shipment
```

Pregled pojedinačne pošiljke:

```http
GET https://localhost:7022/api/Shipment/{shipmentId}
```

Promjena statusa:

```http
PUT https://localhost:7022/api/Shipment/{shipmentId}/status?status=Shipped
```

Mogući statusi su:

```text
Order Received
Shipped
In Transit
Out for Delivery
Delivered
```

## Automatsko testiranje

Projekat sadrži unit, integration i mock testove.

Testovi se nalaze u posebnim test projektima:

```text
CustomerService.Tests
ProductService.Tests
CartService.Tests
PaymentService.Tests
ShipmentService.Tests
```

Testovi se mogu pokrenuti iz Visual Studio-a preko **Test Explorer-a** ili komandnom linijom:

```bash
dotnet test
```

### Unit testovi

Unit testovi provjeravaju pojedinačne dijelove aplikacije izolovano.

Za testiranje dijelova koji koriste bazu koristi se Entity Framework Core In-Memory baza.

Testirano je:

**CustomerService**

* generisanje JWT tokena;
* uspješna registracija;
* odbijanje registracije sa postojećim emailom;
* uspješan login;
* odbijanje login-a sa pogrešnom lozinkom.

**ProductService**

* pronalaženje proizvoda po ID-u;
* vraćanje `404 Not Found` kada proizvod ne postoji.

**PaymentService**

* pronalaženje postojeće narudžbine;
* vraćanje `404 Not Found` kada narudžbina ne postoji;
* dobijanje PayPal access tokena pomoću simuliranog HTTP odgovora.

**ShipmentService**

* pronalaženje pošiljke;
* vraćanje `404 Not Found` kada pošiljka ne postoji;
* provjeru vlasništva nad pošiljkom;
* vraćanje `403 Forbidden` za pošiljku drugog korisnika.

### Integration testovi

Integration testovi provjeravaju rad API-ja kroz ASP.NET Core request pipeline.

Za testiranje se koristi `WebApplicationFactory<Program>`.

Testirano je:

**CustomerService**

* `GET /api/Customer` vraća `200 OK`.

**ProductService**

* `GET /api/Product` vraća `200 OK`.

**CartService**

* pristup korpi bez autentifikacije vraća `401 Unauthorized`.

**PaymentService**

* pristup narudžbinama bez autentifikacije vraća `401 Unauthorized`.

**ShipmentService**

* pristup pošiljkama bez autentifikacije vraća `401 Unauthorized`.

### Mockovi

Mockovi se koriste za simulaciju komunikacije sa drugim servisima i eksternim sistemima.

Kod PaymentService-a koristi se `MockHttpMessageHandler` za simulaciju odgovora PayPal API-ja.

Kod ShipmentService-a koriste se mockovi `PaymentServiceClient`-a i `CustomerServiceClient`-a.

Na ovaj način se kreiranje pošiljke može testirati bez pokretanja stvarnog PaymentService-a i CustomerService-a.

Testirano je da ShipmentService:

1. dobije plaćenu narudžbinu od PaymentService-a;
2. dobije podatke korisnika od CustomerService-a;
3. kreira pošiljku na osnovu dobijenih podataka;
4. sačuva podatke korisnika kao snapshot pošiljke.

## Napomena o ID vrijednostima

ID vrijednosti navedene u primjerima služe samo kao primjer.

Prilikom testiranja potrebno je koristiti ID vrijednosti koje vrate prethodni zahtjevi.

Na primjer, nakon kreiranja korpe servis može vratiti:

```json
{
    "cartId": 14,
    "customerID": 7,
    "totalPrice": 0
}
```

U tom slučaju se vrijednost `14` koristi kao `cartId` u narednim zahtjevima.

Isto pravilo važi za:

```text
CustomerID
ProductID
CartID
CartItemID
OrderID
PaymentID
ShipmentID
```

Zaštićeni endpoint-i zahtijevaju JWT token dobijen prilikom prijave korisnika.

## Važne napomene

* Card i Apple Pay su implementirani kao simulacija procesa plaćanja za potrebe projekta.
* PayPal koristi PayPal Sandbox i ne koristi pravi novac.
* Podaci korisnika se između servisa ne povezuju stranim ključevima u različitim bazama.
* CartService, PaymentService i ShipmentService koriste odvojene fizičke baze.
* CustomerService i ProductService trenutno koriste bazu `Ecommerce`, ali pristupaju samo tabelama koje pripadaju njihovom servisu.
* `Database/Ecommerce.sql` služi za kreiranje potrebnih baza, tabela i početnih podataka.
* Potrebno je prilagoditi lokalne MySQL i PayPal konfiguracione vrijednosti prije pokretanja.
* ID vrijednosti u primjerima nisu fiksne i zavise od trenutnog stanja baze.
