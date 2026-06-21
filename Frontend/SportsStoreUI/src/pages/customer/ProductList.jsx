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
        setCategories(categoryData);
        setBrands(brandData);
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
  // searchParams is the source of truth for the filter state.
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [searchParams]);

  const updateFilter = (name, value) => {
    const next = new URLSearchParams(searchParams);
    if (value) next.set(name, value); else next.delete(name);
    if (name !== 'page') next.set('page', '1');
    setSearchParams(next);
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
              {Array.from({ length: meta.totalPages }, (_, index) => index + 1).map((page) => (
                <button
                  key={page}
                  className={page === filters.page ? 'active' : ''}
                  onClick={() => updateFilter('page', String(page))}
                >
                  {page}
                </button>
              ))}
            </div>
          )}
        </section>
      </div>
    </div>
  );
}
