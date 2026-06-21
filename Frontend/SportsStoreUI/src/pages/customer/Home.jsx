import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import ProductCard from '../../components/ProductCard';
import { productService } from '../../services/productService';
import { aiService } from '../../services/aiService';

export default function Home() {
  const [products, setProducts] = useState([]);
  const [categories, setCategories] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    Promise.all([
      productService.getProducts({ pageSize: 8 }),
      productService.getCategories(),
    ])
      .then(([productData, categoryData]) => {
        setProducts(productData.items || []);
        setCategories(categoryData || []);
      })
      .finally(() => setLoading(false));
  }, []);

  useEffect(() => {
    if (!localStorage.getItem('sports_token')) return;
    aiService.personalized(8)
      .then((items) => {
        if (items?.length) {
          setProducts(items.map((item) => ({
            id: item.productId,
            name: item.productName,
            minPrice: item.price,
            maxPrice: item.price,
            imageUrl: item.imageUrl,
            categoryName: 'AI đề xuất',
            totalStock: 1,
          })));
        }
      })
      .catch(() => {});
  }, []);

  return (
    <>
      <section className="hero-section">
        <div className="container hero-grid">
          <div>
            <div className="eyebrow accent">SPORTS STORE AI</div>
            <h1>Bứt phá mọi giới hạn</h1>
            <p>Khám phá sản phẩm thể thao và nhận gợi ý phù hợp bằng AI.</p>
            <div className="button-row">
              <Link className="btn btn-accent" to="/products">Mua ngay</Link>
              <a className="btn btn-light" href="#ai-products">Khám phá AI</a>
            </div>
          </div>
          <div className="hero-art">
            <div className="hero-badge">AI</div>
            <span>Gợi ý sản phẩm<br />và kích thước</span>
          </div>
        </div>
      </section>

      <section className="section container">
        <div className="section-heading">
          <div>
            <div className="eyebrow">KHÁM PHÁ</div>
            <h2>Danh mục nổi bật</h2>
          </div>
        </div>
        <div className="category-grid">
          {categories.map((category) => (
            <Link
              key={category.id}
              className="category-card"
              to={`/products?categoryId=${category.id}`}
            >
              <div className="category-icon">★</div>
              <strong>{category.name}</strong>
              <span>{category.description}</span>
            </Link>
          ))}
        </div>
      </section>

      <section id="ai-products" className="section container">
        <div className="section-heading">
          <div>
            <div className="eyebrow">AI CÁ NHÂN HÓA</div>
            <h2>Gợi ý dành cho bạn</h2>
          </div>
          <Link to="/products">Xem tất cả</Link>
        </div>
        {loading ? (
          <div className="state-card">Đang tải sản phẩm...</div>
        ) : (
          <div className="product-grid">
            {products.map((product) => <ProductCard key={product.id} product={product} />)}
          </div>
        )}
      </section>
    </>
  );
}
