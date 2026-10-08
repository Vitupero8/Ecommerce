# E-Commerce Microservices

## Opis projekta

Ovaj projekat predstavlja backend e-commerce sistema implementiranog korišćenjem mikroservisne arhitekture.

Sistem je podijeljen na više nezavisnih servisa, gdje svaki servis ima svoju odgovornost i sopstvenu bazu podataka. Servisi međusobno komuniciraju putem HTTP zahtjeva i REST API-ja.

Projekat obuhvata upravljanje korisnicima, proizvodima, korpom, narudžbinama i plaćanjima, kao i kreiranje i praćenje pošiljki.

Za autentifikaciju korisnika koristi se JWT (JSON Web Token), dok je za plaćanje putem PayPal-a korišćen PayPal Sandbox.

## Arhitektura

Projekat se sastoji od sljedećih mikroservisa:

* **CustomerService** – registracija, prijava i upravljanje korisnicima.
* **ProductService** – upravljanje proizvodima, cijenama i stanjem proizvoda.
* **CartService** – kreiranje i upravljanje korpama, dodavanje proizvoda, promjena količine, uklanjanje proizvoda i checkout.
* **PaymentService** – kreiranje narudžbina i obrada različitih načina plaćanja.
* **ShipmentService** – kreiranje i praćenje pošiljki nakon uspješnog plaćanja.

Svaki servis ima sopstvenu bazu podataka. Servisi ne pristupaju direktno bazi podataka drugog servisa, već potrebne podatke dobijaju putem HTTP komunikacije.

Osnovni tok sistema je:

**Customer → Product → Cart → Payment → Shipment**

Primjer toka kupovine:

1. Korisnik se registruje i prijavljuje.
2. Korisnik kreira korpu.
3. Proizvod se dodaje u korpu uz provjeru dostupnog stanja i trenutne cijene.
4. Prilikom checkout-a ponovo se provjeravaju cijene i stanje proizvoda.
5. Kreira se narudžbina u PaymentService-u.
6. Korisnik bira način plaćanja.
7. Nakon uspješnog plaćanja narudžbina dobija status `Paid`.
8. ShipmentService na osnovu plaćene narudžbine kreira pošiljku.
9. Pošiljka prolazi kroz statuse `Order Received`, `Shipped`, `In Transit`, `Out for Delivery` i `Delivered`.

## Korišćene tehnologije

* **C#** – programski jezik korišćen za razvoj backend servisa.
* **.NET 10** – platforma na kojoj su razvijeni mikroservisi.
* **ASP.NET Core Web API** – razvoj REST API-ja i HTTP endpoint-a.
* **Entity Framework Core** – pristup i rad sa bazama podataka.
* **MySQL** – relaciona baza podataka.
* **JWT (JSON Web Token)** – autentifikacija i autorizacija korisnika.
* **PayPal Sandbox** – simulacija i testiranje PayPal plaćanja.
* **Postman** – testiranje REST API endpoint-a.
* **Git / GitHub** – verzionisanje i čuvanje izvornog koda.


## Struktura projekta

Projekat je organizovan kao skup pet nezavisnih mikroservisa. Svaki servis predstavlja zaseban ASP.NET Core Web API projekat i ima sopstvenu bazu podataka.


Ecommerce/
│
├── CustomerService/
├── ProductService/
├── CartService/
├── PaymentService/
├── ShipmentService/
│
├── Postman/
│   └── Ecommerce.postman_collection.json
│
├── README.md
└── Ecommerce.sln
```

### CustomerService

Zadužen je za upravljanje korisnicima.

Omogućava:

* registraciju korisnika,
* prijavu korisnika,
* JWT autentifikaciju,
* pregled i izmjenu podataka korisnika,
* brisanje korisnika.

### ProductService

Zadužen je za upravljanje proizvodima.

Čuva podatke o:

* nazivu proizvoda,
* tipu proizvoda,
* cijeni,
* količini proizvoda na stanju.

Omogućava kreiranje, pregled, izmjenu i brisanje proizvoda.

### CartService

Zadužen je za upravljanje korpom korisnika.

Omogućava:

* kreiranje korpe,
* dodavanje proizvoda,
* provjeru dostupnog stanja proizvoda,
* provjeru trenutne cijene proizvoda,
* promjenu količine,
* uklanjanje proizvoda,
* preračunavanje ukupne cijene,
* checkout korpe.

CartService komunicira sa ProductService-om kako bi dobio podatke o proizvodima, a prilikom checkout-a šalje podatke PaymentService-u.

### PaymentService

Zadužen je za upravljanje narudžbinama i plaćanjem.

Podržani načini plaćanja su:

* Card,
* PayPal,
* Apple Pay,
* Bank Transfer.

Servis kreira narudžbine, obrađuje plaćanja i prati njihov status. PayPal plaćanje je povezano sa PayPal Sandbox okruženjem, dok su Card i Apple Pay implementirani kao simulacija procesa plaćanja.

### ShipmentService

Zadužen je za upravljanje pošiljkama nakon uspješno izvršenog plaćanja.

Prilikom kreiranja pošiljke provjerava da li je narudžbina plaćena, nakon čega preuzima potrebne podatke o korisniku i čuva ih kao podatke vezane za konkretnu pošiljku.

Pošiljka može imati sljedeće statuse:


Order Received
Shipped
In Transit
Out for Delivery
Delivered
```

### Postman

U folderu `Postman` nalazi se kolekcija zahtjeva za testiranje svih servisa.

Kolekcija je organizovana po servisima i omogućava testiranje kompletnog toka:


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

## Baze podataka

Svaki mikroservis koristi sopstvenu bazu podataka. Servisi ne pristupaju direktno tabelama koje pripadaju drugim servisima.

Podaci između servisa razmjenjuju se putem HTTP zahtjeva i REST API-ja.

### CustomerService

Baza:


Ecommerce
```

Tabela:


Customers
```

Tabela sadrži podatke o korisnicima, kao što su ime, prezime, telefon, email, adresa i hash lozinke.

### ProductService

Baza:


Ecommerce
```

Tabela:


Product
```

Tabela sadrži podatke o proizvodima, njihovoj cijeni i količini na stanju.

### CartService

Baza:


CartService
```

Tabele:


Carts
CartItems
```

`Carts` čuva podatke o korpama korisnika i njihovoj ukupnoj cijeni.

`CartItems` čuva proizvode koji se nalaze u pojedinačnoj korpi, njihovu količinu i cijenu u trenutku dodavanja u korpu.

### PaymentService

Baza:


PaymentService
```

Tabele:


Orders
OrderItem
Payments
PaymentMethods
```

`Orders` čuva podatke o narudžbinama.

`OrderItem` čuva proizvode koji pripadaju pojedinačnoj narudžbini.

`Payments` čuva podatke o izvršenim ili neuspješnim pokušajima plaćanja.

`PaymentMethods` čuva sačuvane načine plaćanja korisnika i informaciju o podrazumijevanom načinu plaćanja.

### ShipmentService

Baza:


ShipmentService
```

Tabela:


Shipments
```

Tabela čuva podatke o pošiljkama i njihovom trenutnom statusu.

Podaci o korisniku kao što su ime, prezime, telefon i adresa čuvaju se kao dio konkretne pošiljke. Na ovaj način promjena adrese korisnika kasnije neće promijeniti adresu već kreirane pošiljke.

### Komunikacija između baza

Iako servisi koriste podatke drugih servisa, ne koriste direktne veze između njihovih baza.

Na primjer, CartService ne pristupa tabeli `Product` direktno. Umjesto toga, šalje HTTP zahtjev ProductService-u i od njega dobija podatke o proizvodu.

Na isti način, ShipmentService ne pristupa direktno tabelama PaymentService-a ili CustomerService-a, već potrebne podatke dobija putem njihovih API endpoint-a.

Ovakva organizacija omogućava da svaki servis bude nezavisan i da se njegova baza može mijenjati bez direktnog uticaja na baze ostalih servisa.



## Pokretanje projekta

### Preduslovi

Prije pokretanja projekta potrebno je imati instalirano:

* .NET 10 SDK
* MySQL Server
* Visual Studio ili drugi IDE koji podržava .NET
* Postman, ukoliko se želi testirati API kroz pripremljenu kolekciju

### 1. Preuzimanje projekta

Klonirati repository sa GitHub-a:

```bash
git clone <URL_REPOSITORY-ja>
```

Nakon toga otvoriti `Ecommerce.sln` u Visual Studio-u.

### 2. Kreiranje baza podataka

Prije pokretanja servisa potrebno je kreirati baze podataka i odgovarajuće tabele u MySQL-u.

Projekat koristi sljedeće baze:


Ecommerce
CartService
PaymentService
ShipmentService
```

SQL skripte za kreiranje tabela nalaze se u odgovarajućim servisima/projektnoj dokumentaciji.

### 3. Konfiguracija konekcije sa bazom

Svaki servis mora imati odgovarajuću connection string konfiguraciju za MySQL bazu koju koristi.

Primjer:


Server=localhost;
Port=3306;
Database=CartService;
User=root;
Password=YOUR_PASSWORD;
```

Vrijednosti poput lozinke potrebno je prilagoditi lokalnoj MySQL konfiguraciji.

### 4. PayPal konfiguracija

Za testiranje PayPal plaćanja potrebno je koristiti PayPal Sandbox nalog.

PayPal Client ID i Client Secret ne treba unositi direktno u source code. Potrebno ih je postaviti kroz lokalnu konfiguraciju odnosno User Secrets.

### 5. Pokretanje servisa

Potrebno je pokrenuti svih pet servisa:


CustomerService
ProductService
CartService
PaymentService
ShipmentService
```

Servisi koriste sljedeće HTTPS portove:


CustomerService → https://localhost:7252
ProductService  → https://localhost:7138
CartService     → https://localhost:7211
PaymentService  → https://localhost:7112
ShipmentService → https://localhost:7022
```

Svi servisi moraju biti pokrenuti istovremeno zato što međusobno komuniciraju putem HTTP zahtjeva.

### 6. Provjera servisa

Nakon pokretanja servisa moguće je provjeriti njihove API endpoint-e kroz Swagger/OpenAPI ili Postman.

Za testiranje kompletnog sistema preporučuje se korišćenje pripremljene Postman kolekcije koja se nalazi u:

Postman/Ecommerce.postman_collection.json
```

Detaljan redosljed testiranja opisan je u sekciji **Testiranje pomoću Postman-a**.



## Konfiguracija

Prije pokretanja projekta potrebno je podesiti konekcije sa bazama podataka, JWT autentifikaciju i PayPal Sandbox pristup.

### MySQL

Servisi koriste MySQL Server koji je pokrenut lokalno.

Potrebno je podesiti odgovarajuće podatke za konekciju sa bazom:


Server=localhost;
Port=3306;
Database=NAZIV_BAZE;
User=root;
Password=LOKALNA_MYSQL_LOZINKA;
```

Naziv baze zavisi od servisa:


CustomerService → Ecommerce
ProductService  → Ecommerce
CartService     → CartService
PaymentService  → PaymentService
ShipmentService → ShipmentService
```

`User` i `Password` treba prilagoditi lokalnoj MySQL konfiguraciji.

### JWT

CustomerService koristi JWT za autentifikaciju i autorizaciju korisnika.

Nakon uspješne prijave korisnik dobija JWT token koji se koristi za pristup zaštićenim endpoint-ima.

JWT secret treba biti postavljen kao lokalna konfiguraciona vrijednost i ne treba ga objavljivati u Git repository-ju.

Primjer konfiguracije:

```json id="q1qz0v"
{
  "Jwt": {
    "Key": "YOUR_JWT_SECRET"
  }
}
```

CartService, PaymentService i ShipmentService koriste isti JWT secret kako bi mogli validirati tokene koje je izdao CustomerService.

### PayPal Sandbox

PaymentService koristi PayPal Sandbox za testiranje PayPal plaćanja.

Potrebno je obezbijediti:

PayPal Client ID
PayPal Client Secret
```

Ove vrijednosti se čuvaju lokalno kroz User Secrets ili drugi lokalni način konfiguracije i ne objavljuju se u Git repository-ju.

PayPal Sandbox služi za simulaciju stvarnog procesa plaćanja bez korišćenja pravog novca.

### HTTPS sertifikati

Servisi koriste HTTPS tokom lokalnog razvoja.

Ukoliko Visual Studio ili .NET prijavi problem sa development HTTPS sertifikatom, može se koristiti:

```bash id="w2axf3"
dotnet dev-certs https --trust
```

Nakon konfiguracije baza podataka i potrebnih lokalnih secrets-a, moguće je pokrenuti svih pet servisa i početi sa testiranjem kroz Postman.


## Testiranje pomoću Postman-a

Za testiranje REST API-ja pripremljena je Postman kolekcija:


Postman/Ecommerce.postman_collection.json
```

### Uvoz kolekcije

1. Pokrenuti svih pet servisa.
2. Otvoriti Postman.
3. Izabrati **Import**.
4. Odabrati fajl:


Postman/Ecommerce.postman_collection.json
```

5. Nakon uvoza kolekcija će biti dostupna u Postman-u.

### Redosljed testiranja

Preporučeni redosljed testiranja je:


1. CustomerService
       ↓
2. ProductService
       ↓
3. CartService
       ↓
4. PaymentService
       ↓
5. ShipmentService
```

### 1. CustomerService

Prvo je potrebno registrovati korisnika:


POST https://localhost:7252/api/Customer/register
```

Primjer request body-ja:

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

Nakon registracije izvršiti prijavu:


POST https://localhost:7252/api/Customer/login
```

Primjer request body-ja:

```json
{
    "email": "test@example.com",
    "password": "Test123!"
}
```

Login vraća JWT token koji se koristi za pristup zaštićenim endpoint-ima.

Token je potrebno postaviti u **Authorization → Bearer Token** za zahtjeve koji zahtijevaju autentifikaciju.

### 2. ProductService

ProductService se koristi za pregled i upravljanje proizvodima.

Pregled proizvoda:


GET https://localhost:7138/api/Product
```

Kreiranje proizvoda:


POST https://localhost:7138/api/Product
```

Primjer request body-ja:

```json
{
    "productName": "Test Product",
    "productType": "Electronics",
    "productPrice": 49.99,
    "productStockQuantity": 20
}
```

Nakon kreiranja proizvoda sačuvati njegov `ProductID`, jer će biti potreban za testiranje CartService-a.

### 3. CartService

Prvo je potrebno kreirati korpu:


POST https://localhost:7211/api/Cart
```

Primjer request body-ja:

```json
{
    "totalPrice": 0
}
```

Nakon kreiranja korpe sačuvati njen `CartID`.

Dodavanje proizvoda:


POST https://localhost:7211/api/Cart/product
```

Primjer request body-ja:

```json
{
    "cartId": 1,
    "productId": 1,
    "quantity": 2
}
```

`cartId` i `productId` treba zamijeniti vrijednostima koje su vraćene prilikom prethodnih zahtjeva.

Promjena količine:


PUT https://localhost:7211/api/Cart/{cartItemId}
```

Primjer request body-ja:

```json
{
    "quantity": 3
}
```

Brisanje proizvoda iz korpe:


DELETE https://localhost:7211/api/Cart/{cartItemId}
```

Checkout:


POST https://localhost:7211/api/Cart/{cartId}
```

Checkout ne zahtijeva request body.

Prilikom dodavanja proizvoda CartService provjerava stanje proizvoda i koristi trenutnu cijenu iz ProductService-a.

Prilikom checkout-a cijena i stanje proizvoda ponovo se provjeravaju prije kreiranja narudžbine.

### 4. PaymentService

Nakon checkout-a kreira se narudžbina u PaymentService-u.

Pregled narudžbine:


GET https://localhost:7112/api/Order/{orderId}
```

Moguće je testirati sljedeće načine plaćanja:

#### Card


POST https://localhost:7112/api/Order/{orderId}/card-payment
```

Primjer uspješnog request body-ja:

```json
{
    "cardNumber": "4111111111111111",
    "expiryDate": "12/30",
    "cvv": "123"
}
```

Postoje i simulirani neuspješni scenariji.

#### Apple Pay


POST https://localhost:7112/api/Order/{orderId}/apple-pay
```

Primjer:

```json
{
    "token": "APPLE-PAY-12345"
}
```

#### Bank Transfer

Pokretanje bank transfera:


POST https://localhost:7112/api/Order/{orderId}/bank-transfer
```

Nakon toga se transfer može potvrditi pomoću `paymentId` vrijednosti koju servis vrati:


POST https://localhost:7112/api/Order/bank-transfer/{paymentId}/confirm
```

#### PayPal

Pokretanje PayPal plaćanja:


POST https://localhost:7112/api/Order/{orderId}/payment
```

PaymentService vraća PayPal Sandbox podatke i URL za odobravanje plaćanja.

PayPal plaćanje koristi PayPal Sandbox za simulaciju stvarnog procesa plaćanja.

### 5. ShipmentService

Pošiljka se može kreirati tek nakon uspješnog plaćanja narudžbine.


POST https://localhost:7022/api/Shipment
```

Primjer request body-ja:

```json
{
    "orderID": 1
}
```

ShipmentService provjerava status narudžbine i odbija kreiranje pošiljke ukoliko narudžbina nije plaćena.

Pregled svih pošiljki:


GET https://localhost:7022/api/Shipment
```

Pregled pojedinačne pošiljke:


GET https://localhost:7022/api/Shipment/{shipmentId}
```

Promjena statusa pošiljke:

```text
PUT https://localhost:7022/api/Shipment/{shipmentId}/status?status=Shipped
```

Mogući statusi su:


Order Received
Shipped
In Transit
Out for Delivery
Delivered
```

Primjer kompletnog toka pošiljke:

```text
Order Received
      ↓
Shipped
      ↓
In Transit
      ↓
Out for Delivery
      ↓
Delivered
```

### Napomena o ID vrijednostima

ID vrijednosti navedene u primjerima služe samo kao primjer. Prilikom testiranja potrebno je koristiti ID vrijednosti koje vrate prethodni zahtjevi.

Na primjer, nakon kreiranja korpe servis može vratiti:

```json
{
    "cartId": 14,
    "customerID": 7,
    "totalPrice": 0
}
```

U tom slučaju se vrijednost `14` koristi kao `cartId` u narednim zahtjevima.

Isto pravilo važi za `ProductID`, `CartItemID`, `OrderID`, `PaymentID` i `ShipmentID`.

Zaštićeni endpoint-i zahtijevaju JWT token dobijen prilikom prijave korisnika.


## Testiranje

Projekat koristi više nivoa testiranja kako bi se provjerila ispravnost pojedinačnih komponenti, API endpoint-a i komunikacije između mikroservisa.

### Unit testovi

Unit testovi provjeravaju pojedinačne dijelove aplikacije izolovano, bez korišćenja stvarne MySQL baze ili drugih servisa.

Za testiranje baza podataka koristi se Entity Framework Core In-Memory baza.

Testirana funkcionalnost uključuje:

**CustomerService**

* generisanje JWT tokena;
* uspješnu registraciju korisnika;
* odbijanje registracije sa već postojećim emailom;
* uspješan login;
* odbijanje login-a sa pogrešnom lozinkom.

**ProductService**

* pronalaženje proizvoda po ID-u;
* vraćanje `404 Not Found` odgovora kada proizvod ne postoji.

**PaymentService**

* pronalaženje postojeće narudžbine;
* vraćanje `404 Not Found` odgovora kada narudžbina ne postoji;
* dobijanje PayPal access tokena pomoću simuliranog HTTP odgovora.

**ShipmentService**

* pronalaženje pošiljke;
* vraćanje `404 Not Found` odgovora kada pošiljka ne postoji;
* provjeru vlasništva nad pošiljkom i vraćanje `403 Forbidden` odgovora za drugog korisnika.

### Integration testovi

Integration testovi provjeravaju rad API-ja kroz stvarni ASP.NET Core request pipeline.

Za testiranje se koristi `WebApplicationFactory<Program>`.

Testirani su sljedeći scenariji:

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

Kod `PaymentService`-a koristi se `MockHttpMessageHandler` za simulaciju odgovora PayPal API-ja. Na ovaj način se može testirati dobijanje PayPal access tokena bez stvarnog HTTP zahtjeva prema PayPal Sandbox-u.

Kod `ShipmentService`-a koriste se mockovi `PaymentServiceClient`-a i `CustomerServiceClient`-a. Na ovaj način se kreiranje pošiljke može testirati bez pokretanja stvarnog PaymentService-a i CustomerService-a.

Testirano je da ShipmentService:

1. dobije plaćenu narudžbinu od PaymentService-a;
2. dobije podatke korisnika od CustomerService-a;
3. kreira pošiljku na osnovu dobijenih podataka;
4. sačuva podatke korisnika kao snapshot pošiljke.

### Postman testiranje

Pored automatskih testova, REST API endpoint-i su ručno testirani pomoću Postman-a.

Postman kolekcija omogućava testiranje glavnog toka sistema:

`Customer → Product → Cart → Payment → Shipment`

Testirani su uspješni i neuspješni scenariji, uključujući:

* registraciju i prijavu korisnika;
* JWT autentifikaciju;
* CRUD operacije;
* provjeru stanja i cijene proizvoda;
* upravljanje korpom;
* checkout;
* Card, PayPal, Apple Pay i Bank Transfer plaćanja;
* neuspješna plaćanja;
* kreiranje i potvrdu pošiljke;
* promjenu statusa pošiljke.






