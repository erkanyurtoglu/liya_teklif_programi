-- Admin
CREATE TABLE AdminGiris (
    AdminID INT IDENTITY(1,1) PRIMARY KEY,
    AdminKullaniciAdi NVARCHAR(20) NOT NULL,
    AdminSifre NVARCHAR(20) NOT NULL,
);

-- Firmalar
CREATE TABLE Firmalar (
    FirmaKoduID INT IDENTITY(1,1) PRIMARY KEY,
    FirmaAdi NVARCHAR(250) NOT NULL,
    Adres NVARCHAR(500),
    Telefon NVARCHAR(20),
    Email NVARCHAR(70)
);

-- Personeller
CREATE TABLE Personeller (
    PersonelKoduID INT IDENTITY(1,1) PRIMARY KEY,
    AdSoyad NVARCHAR(50) NOT NULL,
    Pozisyon NVARCHAR(50),
    Telefon NVARCHAR(20),
    PersonelSifre NVARCHAR(20)
);

-- Urunler
CREATE TABLE Urunler (
    UrunKoduID NVARCHAR(30) PRIMARY KEY,
    Kategori NVARCHAR(100),
    Aciklama NVARCHAR(max),
    Adet INT,
    BirimSatisFiyati DECIMAL(10,2),
    SatisToplamFiyati DECIMAL(10,2),
    YurticiMaliyet DECIMAL(10,2),
    ToplamFiyat DECIMAL(10,2)
);

-- Teklifler
CREATE TABLE Teklifler (
    TeklifNoID INT IDENTITY(1,1) PRIMARY KEY,
    FirmaKoduID INT NOT NULL,
    PersonelKoduID INT NOT NULL,
    TeklifTarihi DATETIME DEFAULT GETDATE(),
    ToplamTutar DECIMAL(10,2),
    FOREIGN KEY (FirmaKoduID) REFERENCES Firmalar(FirmaKoduID),
    FOREIGN KEY (PersonelKoduID) REFERENCES Personeller(PersonelKoduID)
);

-- TeklifDetaylari
CREATE TABLE TeklifDetaylari (
    DetayID INT IDENTITY(1,1) PRIMARY KEY,
    TeklifNoID INT NOT NULL,
    UrunKoduID NVARCHAR(30) NOT NULL,
    Adet INT,
    BirimFiyat DECIMAL(10,2),
    ToplamFiyat DECIMAL(10,2),
    FOREIGN KEY (TeklifNoID) REFERENCES Teklifler(TeklifNoID),
    FOREIGN KEY (UrunKoduID) REFERENCES Urunler(UrunKoduID)
);
