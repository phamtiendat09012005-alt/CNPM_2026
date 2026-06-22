import { useEffect, useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import ProductCard from '../../components/ProductCard';
import { productService } from '../../services/productService';

export default function ProductList() {
  const [searchParams, setSearchParams] = useSearchParams();
  const [products, setProducts] = useState([]);
  const [categories, setCategories] = useState([]);
  const [brands, setBrands] = useState([]);
  const [meta, setMeta] = useState({ totalItems: 0, totalPages: 0 });
  const [loading, setLoading] = useState(true);

  const filters = {
    keyword: searchParams.get('keyword') || '',
    categoryId: searchParams.get('categoryId') || '',
    brandId: searchParams.get('brandId') || '',
    minPrice: searchParams.get('minPrice') || '',
    maxPrice: searchParams.get('maxPrice') || '',
    sort: searchParams.get('sort') || 'newest',
    page: Number(searchParams.get('page') || 1),
  };

  useEffect(() => {
    Promise.all([productService.getCategories(), productService.getBrands()])
      .then(([categoryData, brandData]) => {
        setCategories(categoryData || []);
        setBrands(brandData || []);
      });
  }, []);

  useEffect(() => {
    setLoading(true);
    productService.getProducts({ ...filters, pageSize: 12 })
      .then((data) => {
        setProducts(data.items || []);
        setMeta(data);
      })
      .finally(() => setLoading(false));
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [searchParams]);

  const updateFilter = (name, value) => {
    const next = new URLSearchParams(searchParams);
    if (value) next.set(name, value); else next.delete(name);
    if (name !== 'page') next.set('page', '1');
    setSearchParams(next);
  };

  // Thuật toán hiển thị phân trang thông minh (Rút gọn cho Big Data)
  const getPaginationGroup = () => {
    const total = meta.totalPages;
    const current = filters.page;
    if (total <= 7) return Array.from({ length: total }, (_, i) => i + 1);

    if (current <= 4) return [1, 2, 3, 4, 5, '...', total];
    if (current >= total - 3) return [1, '...', total - 4, total - 3, total - 2, total - 1, total];

    return [1, '...', current - 1, current, current + 1, '...', total];
  };

  return (
    <div className="container section">
      <div className="page-title-row">
        <div>
          <div className="eyebrow">CỬA HÀNG</div>
          <h1>Sản phẩm thể thao</h1>
        </div>
        <span>{meta.totalItems || 0} sản phẩm</span>
      </div>

      <div className="catalog-layout">
        <aside className="filter-panel">
          <h3>Bộ lọc</h3>
          <label>Từ khóa</label>
          <input
            value={filters.keyword}
            onChange={(e) => updateFilter('keyword', e.target.value)}
            placeholder="Tên sản phẩm..."
          />
          <label>Danh mục</label>
          <select value={filters.categoryId} onChange={(e) => updateFilter('categoryId', e.target.value)}>
            <option value="">Tất cả</option>
            {categories.map((item) => <option key={item.id} value={item.id}>{item.name}</option>)}
          </select>
          <label>Thương hiệu</label>
          <select value={filters.brandId} onChange={(e) => updateFilter('brandId', e.target.value)}>
            <option value="">Tất cả</option>
            {brands.map((item) => <option key={item.id} value={item.id}>{item.name}</option>)}
          </select>
          <label>Giá tối thiểu</label>
          <input type="number" value={filters.minPrice} onChange={(e) => updateFilter('minPrice', e.target.value)} />
          <label>Giá tối đa</label>
          <input type="number" value={filters.maxPrice} onChange={(e) => updateFilter('maxPrice', e.target.value)} />
          <button className="btn btn-ghost btn-block" onClick={() => setSearchParams({})}>Xóa bộ lọc</button>
        </aside>

        <section>
          <div className="catalog-toolbar">
            <select value={filters.sort} onChange={(e) => updateFilter('sort', e.target.value)}>
              <option value="newest">Mới nhất</option>
              <option value="price_asc">Giá tăng dần</option>
              <option value="price_desc">Giá giảm dần</option>
              <option value="name">Tên A-Z</option>
            </select>
          </div>
          
          {loading ? (
            <div className="state-card">Đang tải sản phẩm...</div>
          ) : products.length === 0 ? (
            <div className="state-card">Không tìm thấy sản phẩm phù hợp.</div>
          ) : (
            <div className="product-grid product-grid-3">
              {products.map((product) => <ProductCard key={product.id} product={product} />)}
            </div>
          )}

          {meta.totalPages > 1 && (
            <div className="pagination">
              {/* Nút lùi về trang trước */}
              <button
                disabled={filters.page === 1}
                onClick={() => updateFilter('page', String(filters.page - 1))}
              >
                &laquo;
              </button>

              {/* Các nút phân trang */}
              {getPaginationGroup().map((item, index) => (
                <button
                  key={index}
                  className={item === filters.page ? 'active' : ''}
                  onClick={() => item !== '...' && updateFilter('page', String(item))}
                  disabled={item === '...'}
                  style={item === '...' ? { cursor: 'default', background: 'transparent', border: 'none' } : {}}
                >
                  {item}
                </button>
              ))}

              {/* Nút tiến lên trang sau */}
              <button
                disabled={filters.page === meta.totalPages}
                onClick={() => updateFilter('page', String(filters.page + 1))}
              >
                &raquo;
              </button>
            </div>
          )}
        </section>
      </div>
    </div>
  );
}