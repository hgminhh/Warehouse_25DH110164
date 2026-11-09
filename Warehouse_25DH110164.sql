    COUNT(*) AS TongSoSanPham,
    SUM(CASE WHEN Price >= 500000 THEN 1 ELSE 0 END) AS SoSanPhamGiaCao,
    COALESCE(SUM(Price * Quantity), 0) AS TongGiaTriTonKho
FROM dbo.Products;
GO

/*
    CAU 7.2 - LENH TEST DU LIEU
    Chi chay neu muon dat ProductID 1 va 2 theo du lieu kiem thu.
    Lenh nay thay doi du lieu hien co, can kiem tra ID truoc khi chay.

    UPDATE dbo.Products
    SET Price = 100000, Quantity = 2
    WHERE ProductID = 1;

    UPDATE dbo.Products
    SET Price = 50000, Quantity = 3
    WHERE ProductID = 2;

    UPDATE dbo.Products
    SET Quantity = 0
    WHERE ProductID NOT IN (1, 2);

    SELECT ProductID, ProductName, Price, Quantity,
           Price * Quantity AS GiaTriTonKho
    FROM dbo.Products
    ORDER BY ProductID;
*/

-- Cau 7.3 - LENH TEST CAP NHAT GIA (vi du ProductID = 1).
-- Bo dau comment va chay rieng neu muon kiem tra thay doi thong ke.
-- UPDATE dbo.Products SET Price = 600000 WHERE ProductID = 1;
-- GO

-- Chay lai truy van thong ke sau khi thay doi du lieu.
SELECT
    COUNT(*) AS TongSoSanPham,
    SUM(CASE WHEN Price >= 500000 THEN 1 ELSE 0 END) AS SoSanPhamGiaCao,
    COALESCE(SUM(Price * Quantity), 0) AS TongGiaTriTonKho
FROM dbo.Products;
GO