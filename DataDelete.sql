-- Orders tablosundaki verileri sil
DELETE FROM Orders;
DBCC CHECKIDENT ('Orders', RESEED, 0);

-- Customers tablosundaki verileri sil
DELETE FROM Customers;
DBCC CHECKIDENT ('Customers', RESEED, 0);

-- Logs tablosundaki verileri sil
DELETE FROM Logs;
DBCC CHECKIDENT ('Logs', RESEED, 0);

-- Products tablosundaki verileri sil
DELETE FROM Products;
DBCC CHECKIDENT ('Products', RESEED, 0);

--alt sat�rdakileri ayr� ayr� i�le
--EXEC sp_MSforeachtable 'TRUNCATE TABLE ?';
--EXEC sp_MSforeachtable 'DELETE FROM ?';
--EXEC sp_MSforeachtable 'ALTER TABLE ? CHECK CONSTRAINT ALL';