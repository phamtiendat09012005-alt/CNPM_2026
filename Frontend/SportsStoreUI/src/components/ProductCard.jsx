import { Link } from 'react-router-dom';

const money = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' });

export default function ProductCard({ product }) {
  return (
    <article className="product-card">
      <Link to={`/product/${product.id}`} className="product-image-wrap">
        <img
          className="product-image"
          src={product.imageUrl || 'https://placehold.co/600x600?text=Sports+Store'}
          alt={product.name}
        />
      </Link>
      <div className="product-card-body">
        <div className="eyebrow">{product.categoryName}</div>
        <Link className="product-title" to={`/product/${product.id}`}>{product.name}</Link>
        <div className="product-meta">
          <strong>{money.format(product.minPrice)}</strong>
          <span className={product.totalStock > 0 ? 'stock-ok' : 'stock-out'}>
            {product.totalStock > 0 ? `Còn ${product.totalStock}` : 'Hết hàng'}
          </span>
        </div>
      </div>
    </article>
  );
}
