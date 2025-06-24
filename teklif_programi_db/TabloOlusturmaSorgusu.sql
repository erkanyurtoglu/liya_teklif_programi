-- Veritabaný kullaným
USE TeklifSistemiDB;
GO

-- Firmalar tablosu
CREATE TABLE Firmalar (
    FirmaKoduID INT IDENTITY(1,1) PRIMARY KEY,
    FirmaAdi NVARCHAR(100) NOT NULL,
    Adres NVARCHAR(255),
    Telefon NVARCHAR(20),
    Email NVARCHAR(100)
);
GO

-- Personeller tablosu
CREATE TABLE Personeller (
    PersonelKoduID INT IDENTITY(1,1) PRIMARY KEY,
    AdSoyad NVARCHAR(100) NOT NULL,
    Pozisyon NVARCHAR(100),
    Telefon NVARCHAR(20)
);
GO

-- Urunler tablosu (UrunKoduID elle girilecek)
CREATE TABLE Urunler (
    UrunKoduID NVARCHAR(50) PRIMARY KEY,
    Kategori NVARCHAR(100),
    Aciklama NVARCHAR(255),
    Adet INT,
    BirimSatisFiyati DECIMAL(10,2),
    SatisToplamFiyati DECIMAL(10,2),
    YurticiMaliyet DECIMAL(10,2),
    ToplamFiyat DECIMAL(10,2)
);
GO

-- Teklifler tablosu
CREATE TABLE Teklifler (
    TeklifNoID INT IDENTITY(1,1) PRIMARY KEY,
    FirmaKoduID INT NOT NULL,
    PersonelKoduID INT NOT NULL,
    TeklifTarihi DATETIME DEFAULT GETDATE(),
    ToplamTutar DECIMAL(10,2),
    FOREIGN KEY (FirmaKoduID) REFERENCES Firmalar(FirmaKoduID),
    FOREIGN KEY (PersonelKoduID) REFERENCES Personeller(PersonelKoduID)
);
GO

-- TeklifDetaylari tablosu
CREATE TABLE TeklifDetaylari (
    DetayID INT IDENTITY(1,1) PRIMARY KEY,
    TeklifNoID INT NOT NULL,
    UrunKoduID NVARCHAR(50) NOT NULL,
    Adet INT,
    BirimFiyat DECIMAL(10,2),
    ToplamFiyat DECIMAL(10,2),
    FOREIGN KEY (TeklifNoID) REFERENCES Teklifler(TeklifNoID),
    FOREIGN KEY (UrunKoduID) REFERENCES Urunler(UrunKoduID)
);
GO
