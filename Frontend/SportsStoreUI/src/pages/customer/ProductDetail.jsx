import { useEffect, useMemo, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import ProductCard from '../../components/ProductCard';
import { productService } from '../../services/productService';
import { cartService } from '../../services/cartService';
import { aiService } from '../../services/aiService';
import { getErrorMessage } from '../../services/api';

const money = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' });

export default function ProductDetail() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [product, setProduct] = useState(null);
  const [selectedVariantId, setSelectedVariantId] = useState('');
  const [quantity, setQuantity] = useState(1);
  const [related, setRelated] = useState([]);
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');
  const [sizeInput, setSizeInput] = useState({ footLengthCm: '', heightCm: '', weightKg: '', chestCm: '', waistCm: '', preferredFit: 'Regular' });
  const [sizeResult, setSizeResult] = useState(null);

  useEffect(() => {
    Promise.all([
      productService.getProduct(id),
      aiService.related(id, 4),
    ]).then(([productData, relatedData]) => {
      setProduct(productData);
      setSelectedVariantId(String(productData.variants?.find((x) => x.stockQuantity > 0)?.id || ''));
      setRelated((relatedData || []).map((item) => ({
        id: item.productId,
        name: item.productName,
        minPrice: item.price,
        maxPrice: item.price,
        imageUrl: item.imageUrl,
        categoryName: 'Sản phẩm liên quan',
        totalStock: 1,
      })));
    }).catch((requestError) => setError(getErrorMessage(requestError)));

    const sessionId = localStorage.getItem('sports_session') || crypto.randomUUID();
    localStorage.setItem('sports_session', sessionId);
    aiService.recordView(id, sessionId).catch(() => {});
  }, [id]);

  const selectedVariant = useMemo(
    () => product?.variants?.find((item) => item.id === Number(selectedVariantId)),
    [product, selectedVariantId],
  );

  const addToCart = async () => {
    if (!localStorage.getItem('sports_token')) {
      navigate('/login', { state: { from: `/product/${id}` } });
      return;
    }
    setError('');
    setMessage('');
    if (!selectedVariant) {
      setError('Vui lòng chọn biến thể sản phẩm.');
      return;
    }
    try {
      await cartService.addItem(selectedVariant.id, quantity);
      setMessage('Đã thêm sản phẩm vào giỏ hàng.');
    } catch (requestError) {
      setError(getErrorMessage(requestError));
    }
  };

  const recommendSize = async () => {
    setError('');
    setSizeResult(null);
    try {
      const payload = { productId: Number(id), preferredFit: sizeInput.preferredFit };
      Object.entries(sizeInput).forEach(([key, value]) => {
        if (key !== 'preferredFit' && value !== '') payload[key] = Number(value);
      });
      setSizeResult(await aiService.recommendSize(payload));
    } catch (requestError) {
      setError(getErrorMessage(requestError));
    }
  };

  if (error && !product) return <div className="container section"><div className="alert alert-error">{error}</div></div>;
  if (!product) return <div className="container section"><div className="state-card">Đang tải sản phẩm...</div></div>;

  return (
    <div className="container section">
      <div className="breadcrumb"><Link to="/products">Sản phẩm</Link> / {product.name}</div>
      <div className="product-detail-grid">
        <div>
          <img
            className="detail-main-image"
            src={product.images?.[0] || 'https://placehold.co/800x800?text=Sports+Store'}
            alt={product.name}
          />
          <div className="thumbnail-row">
            {(product.images || []).map((image) => <img key={image} src={image} alt={product.name} />)}
          </div>
        </div>
        <div className="product-info-panel">
          <div className="eyebrow">{product.categoryName} · {product.brandName || 'Không thương hiệu'}</div>
          <h1>{product.name}</h1>
          <p>{product.description}</p>
          <div className="detail-price">{money.format(selectedVariant?.price ?? product.basePrice)}</div>
          <label>Chọn biến thể</label>
          <div className="variant-grid">
            {product.variants.map((variant) => (
              <button
                key={variant.id}
                type="button"
                disabled={variant.stockQuantity <= 0}
                className={String(variant.id) === selectedVariantId ? 'variant active' : 'variant'}
                onClick={() => setSelectedVariantId(String(variant.id))}
              >
                {variant.size || 'Mặc định'} / {variant.color || 'Mặc định'}
                <small>{variant.stockQuantity > 0 ? `Còn ${variant.stockQuantity}` : 'Hết hàng'}</small>
              </button>
            ))}
          </div>
          <label>Số lượng</label>
          <input
            className="quantity-input"
            type="number"
            min="1"
            max={selectedVariant?.stockQuantity || 1}
            value={quantity}
            onChange={(e) => setQuantity(Math.max(1, Number(e.target.value)))}
          />
          {message && <div className="alert alert-success">{message}</div>}
          {error && <div className="alert alert-error">{error}</div>}
          <button className="btn btn-primary btn-block" onClick={addToCart}>Thêm vào giỏ hàng</button>

          {(product.productType === 'Shoes' || product.productType === 'Clothing') && (
            <div className="ai-size-box">
              <h3>AI gợi ý kích thước</h3>
              {product.productType === 'Shoes' ? (
                <input
                  type="number"
                  step="0.1"
                  placeholder="Chiều dài bàn chân (cm)"
                  value={sizeInput.footLengthCm}
                  onChange={(e) => setSizeInput({ ...sizeInput, footLengthCm: e.target.value })}
                />
              ) : (
                <div className="form-grid-2">
                  <input type="number" placeholder="Chiều cao (cm)" value={sizeInput.heightCm} onChange={(e) => setSizeInput({ ...sizeInput, heightCm: e.target.value })} />
                  <input type="number" placeholder="Cân nặng (kg)" value={sizeInput.weightKg} onChange={(e) => setSizeInput({ ...sizeInput, weightKg: e.target.value })} />
                  <input type="number" placeholder="Vòng ngực (cm)" value={sizeInput.chestCm} onChange={(e) => setSizeInput({ ...sizeInput, chestCm: e.target.value })} />
                  <input type="number" placeholder="Vòng eo (cm)" value={sizeInput.waistCm} onChange={(e) => setSizeInput({ ...sizeInput, waistCm: e.target.value })} />
                </div>
              )}
              <select value={sizeInput.preferredFit} onChange={(e) => setSizeInput({ ...sizeInput, preferredFit: e.target.value })}>
                <option value="Slim">Ôm</option>
                <option value="Regular">Vừa</option>
                <option value="Loose">Rộng</option>
              </select>
              <button className="btn btn-accent" type="button" onClick={recommendSize}>Nhận gợi ý AI</button>
              {sizeResult && (
                <div className="ai-result">
                  <strong>Size đề xuất: {sizeResult.recommendedSize}</strong>
                  <span>Độ tin cậy: {sizeResult.confidence}%</span>
                  <p>{sizeResult.reason}</p>
                  <small>{sizeResult.disclaimer}</small>
                </div>
              )}
            </div>
          )}
        </div>
      </div>

      {related.length > 0 && (
        <section className="section compact-section">
          <div className="section-heading"><h2>Sản phẩm liên quan</h2></div>
          <div className="product-grid">{related.map((item) => <ProductCard key={item.id} product={item} />)}</div>
        </section>
      )}
    </div>
  );
}
