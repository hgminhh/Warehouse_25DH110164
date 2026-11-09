
using System;
using System.Linq;
using System.Web.Mvc;
using Warehouse_25DH110164.Models;

namespace Warehouse_25DH110164.Controllers
{
    public class ProductController : Controller
    {
        // CÂU 1: DANH SÁCH SẢN PHẨM
        public ActionResult Index()
        {
            using (var db = new Warehouse_25DH110164Entities())
            {
                var products = db.Products.ToList();
                return View(products);
            }
        }

        // CÂU 2: DANH SÁCH CHỈNH SỬA
        public ActionResult EditList()
        {
            using (var db = new Warehouse_25DH110164Entities())
            {
                var products = db.Products.ToList();
                return View(products);
            }
        }

        // CÂU 2: MỞ TRANG CHỈNH SỬA
        [HttpGet]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("EditList");
            }

            using (var db = new Warehouse_25DH110164Entities())
            {
                var product = db.Products.Find(id.Value);

                if (product == null)
                {
                    return HttpNotFound();
                }

                return View(product);
            }
        }

        // CÂU 2: LƯU THÔNG TIN CHỈNH SỬA
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            int ProductID,
            string ProductName,
            string Category,
            decimal Price)
        {
            if (string.IsNullOrWhiteSpace(ProductName))
            {
                ModelState.AddModelError(
                    "ProductName",
                    "Tên sản phẩm không được để trống."
                );
            }

            if (string.IsNullOrWhiteSpace(Category))
            {
                ModelState.AddModelError(
                    "Category",
                    "Danh mục không được để trống."
                );
            }

            if (Price <= 0)
            {
                ModelState.AddModelError(
                    "Price",
                    "Giá sản phẩm phải lớn hơn 0."
                );
            }

            using (var db = new Warehouse_25DH110164Entities())
            {
                var product = db.Products.Find(ProductID);

                if (product == null)
                {
                    return HttpNotFound();
                }

                if (!ModelState.IsValid)
                {
                    product.ProductName = ProductName;
                    product.Category = Category;
                    product.Price = Price;

                    return View(product);
                }

                product.ProductName = ProductName;
                product.Category = Category;
                product.Price = Price;

                db.SaveChanges();
            }

            TempData["Success"] = "Cập nhật sản phẩm thành công!";

            return RedirectToAction("EditList");
        }

        // CÂU 3: LỌC VÀ SẮP XẾP SẢN PHẨM
        [HttpGet]
        public ActionResult Filter(
            string keyword,
            string category,
            string sortOrder)
        {
            using (var db = new Warehouse_25DH110164Entities())
            {
                var products = db.Products.AsQueryable();

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    products = products.Where(p =>
                        p.ProductName.Contains(keyword));
                }

                if (!string.IsNullOrWhiteSpace(category))
                {
                    products = products.Where(p =>
                        p.Category == category);
                }

                switch (sortOrder)
                {
                    case "price_asc":
                        products = products.OrderBy(p => p.Price);
                        break;

                    case "price_desc":
                        products = products.OrderByDescending(p => p.Price);
                        break;

                    case "name_asc":
                        products = products.OrderBy(p => p.ProductName);
                        break;

                    case "name_desc":
                        products = products.OrderByDescending(p => p.ProductName);
                        break;
                }

                ViewBag.Keyword = keyword;
                ViewBag.Category = category;
                ViewBag.SortOrder = sortOrder;

                ViewBag.Categories = db.Products
                    .Select(p => p.Category)
                    .Distinct()
                    .ToList();

                return View(products.ToList());
            }
        }

        // CÂU 4: HIỂN THỊ TRANG ĐIỀU CHỈNH GIÁ
        [HttpGet]
        public ActionResult Adjust()
        {
            using (var db = new Warehouse_25DH110164Entities())
            {
                ViewBag.Products = db.Products
                    .OrderBy(p => p.ProductName)
                    .ToList();

                return View();
            }
        }

        // CÂU 4: XỬ LÝ TĂNG / GIẢM GIÁ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Adjust(
            int? ProductID,
            string adjustType,
            decimal? percentage)
        {
            using (var db = new Warehouse_25DH110164Entities())
            {
                ViewBag.Products = db.Products
                    .OrderBy(p => p.ProductName)
                    .ToList();

                ViewBag.SelectedProductID = ProductID;
                ViewBag.AdjustType = adjustType;
                ViewBag.Percentage = percentage;

                if (ProductID == null)
                {
                    ModelState.AddModelError("", "Vui lòng chọn sản phẩm.");
                }

                var product = ProductID == null
                    ? null
                    : db.Products.Find(ProductID.Value);

                if (ProductID != null && product == null)
                {
                    ModelState.AddModelError("", "Sản phẩm không tồn tại.");
                }

                if (percentage == null || percentage <= 0)
                {
                    ModelState.AddModelError("", "Tỷ lệ phải lớn hơn 0.");
                }
                else if (adjustType == "Increase" && percentage > 100)
                {
                    ModelState.AddModelError("", "Tỷ lệ tăng không được vượt quá 100%.");
                }
                else if (adjustType == "Decrease" && percentage >= 100)
                {
                    ModelState.AddModelError("", "Tỷ lệ giảm phải nhỏ hơn 100%.");
                }
                else if (adjustType != "Increase" && adjustType != "Decrease")
                {
                    ModelState.AddModelError("", "Vui lòng chọn cách điều chỉnh giá.");
                }

                if (!ModelState.IsValid)
                {
                    return View();
                }

                decimal oldPrice = product.Price;
                decimal newPrice;

                if (adjustType == "Increase")
                {
                    newPrice = oldPrice * (1 + percentage.Value / 100);
                }
                else
                {
                    newPrice = oldPrice * (1 - percentage.Value / 100);
                }

                newPrice = Math.Round(
                    newPrice,
                    2,
                    MidpointRounding.AwayFromZero
                );

                if (adjustType == "Decrease" && newPrice < 1000)
                {
                    ModelState.AddModelError(
                        "",
                        "Giá sau giảm không được nhỏ hơn 1.000 đồng."
                    );

                    return View();
                }

                product.Price = newPrice;
                db.SaveChanges();

                TempData["Success"] = "Điều chỉnh giá thành công!";
                TempData["NewPrice"] = newPrice.ToString("N2");

                return RedirectToAction("Adjust");
            }
        }

        // CÂU 5: HIỂN THỊ TRANG GIẢM GIÁ HAI SẢN PHẨM
        [HttpGet]
        public ActionResult Transaction()
        {
            using (var db = new Warehouse_25DH110164Entities())
            {
                ViewBag.Products = db.Products
                    .OrderBy(p => p.ProductName)
                    .ToList();

                return View();
            }
        }

        // CÂU 5: GIẢM GIÁ HAI SẢN PHẨM BẰNG TRANSACTION
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Transaction(
            int? ProductID1,
            int? ProductID2,
            decimal? Percent1,
            decimal? Percent2)
        {
            using (var db = new Warehouse_25DH110164Entities())
            {
                ViewBag.Products = db.Products
                    .OrderBy(p => p.ProductName)
                    .ToList();

                ViewBag.ProductID1 = ProductID1;
                ViewBag.ProductID2 = ProductID2;
                ViewBag.Percent1 = Percent1;
                ViewBag.Percent2 = Percent2;

                if (ProductID1 == null || ProductID2 == null)
                {
                    ModelState.AddModelError("", "Vui lòng chọn đủ hai sản phẩm.");
                }
                else if (ProductID1 == ProductID2)
                {
                    ModelState.AddModelError("", "Phải chọn hai sản phẩm khác nhau.");
                }

                if (Percent1 == null || Percent1 <= 0 || Percent1 >= 100)
                {
                    ModelState.AddModelError(
                        "",
                        "Tỷ lệ giảm sản phẩm A phải lớn hơn 0 và nhỏ hơn 100%."
                    );
                }

                if (Percent2 == null || Percent2 <= 0 || Percent2 >= 100)
                {
                    ModelState.AddModelError(
                        "",
                        "Tỷ lệ giảm sản phẩm B phải lớn hơn 0 và nhỏ hơn 100%."
                    );
                }

                if (!ModelState.IsValid)
                {
                    return View();
                }

                var product1 = db.Products.Find(ProductID1.Value);
                var product2 = db.Products.Find(ProductID2.Value);

                if (product1 == null || product2 == null)
                {
                    ModelState.AddModelError("", "Không tìm thấy sản phẩm.");
                    return View();
                }

                decimal newPrice1 = Math.Round(
                    product1.Price * (1 - Percent1.Value / 100),
                    2,
                    MidpointRounding.AwayFromZero
                );

                decimal newPrice2 = Math.Round(
                    product2.Price * (1 - Percent2.Value / 100),
                    2,
                    MidpointRounding.AwayFromZero
                );

                if (newPrice1 < 1000 || newPrice2 < 1000)
                {
                    ModelState.AddModelError(
                        "",
                        "Giao dịch bị từ chối! Giá sau giảm của cả hai sản phẩm phải từ 1.000 đồng trở lên."
                    );

                    return View();
                }

                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        product1.Price = newPrice1;
                        product2.Price = newPrice2;

                        db.SaveChanges();

                        transaction.Commit();

                        TempData["Success"] = "Giảm giá hai sản phẩm thành công!";
                        TempData["Result1"] = product1.ProductName + ": " + newPrice1.ToString("N2") + " đ";
                        TempData["Result2"] = product2.ProductName + ": " + newPrice2.ToString("N2") + " đ";
                    }
                    catch
                    {
                        transaction.Rollback();

                        TempData["Error"] = "Giao dịch thất bại! Không có sản phẩm nào được cập nhật.";
                    }
                }

                return RedirectToAction("Transaction");
            }
        }

        // CÂU 6: HIỂN THỊ TRANG THÊM SẢN PHẨM
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        // CÂU 6: XỬ LÝ THÊM SẢN PHẨM
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            string ProductName,
            string Category,
            decimal? Price,
            int? Quantity)
        {
            if (string.IsNullOrWhiteSpace(ProductName))
            {
                ModelState.AddModelError(
                    "ProductName",
                    "Tên sản phẩm không được để trống."
                );
            }

            if (string.IsNullOrWhiteSpace(Category))
            {
                ModelState.AddModelError(
                    "Category",
                    "Danh mục không được để trống."
                );
            }

            if (Price == null || Price <= 0)
            {
                ModelState.AddModelError(
                    "Price",
                    "Giá sản phẩm phải lớn hơn 0."
                );
            }

            if (Quantity == null || Quantity < 0)
            {
                ModelState.AddModelError(
                    "Quantity",
                    "Số lượng phải là số nguyên không âm."
                );
            }

            if (!ModelState.IsValid)
            {
                return View();
            }

            using (var db = new Warehouse_25DH110164Entities())
            {
                var product = new Product
                {
                    ProductName = ProductName.Trim(),
                    Category = Category.Trim(),
                    Price = Price.Value,
                    Quantity = Quantity.Value
                };

                db.Products.Add(product);
                db.SaveChanges();
            }

            TempData["Success"] = "Thêm sản phẩm thành công!";

            return RedirectToAction("Index");
        }

        // CÂU 7: THỐNG KÊ GIÁ VÀ GIÁ TRỊ HÀNG HÓA
        [HttpGet]
        public ActionResult Stats()
        {
            using (var db = new Warehouse_25DH110164Entities())
            {
                // Tổng số sản phẩm
                ViewBag.TotalProducts = db.Products.Count();

                // Số sản phẩm có giá từ 500.000 đồng trở lên
                ViewBag.HighPriceCount = db.Products
                    .Count(p => p.Price >= 500000);

                // Tổng giá trị tồn kho
                ViewBag.InventoryValue = db.Products
                    .Sum(p => (decimal?)(p.Price * p.Quantity)) ?? 0m;

                return View();
            }
        }
    }
}
