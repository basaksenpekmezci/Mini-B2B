// Ürün detayını popup (Bootstrap modal) içinde göstermek için AJAX ile partial view yükler.
(function () {
    var modalEl = document.getElementById('productDetailModal');
    if (!modalEl) return;

    var modalBody = document.getElementById('productDetailModalBody');
    var modal = new bootstrap.Modal(modalEl);

    document.addEventListener('click', function (e) {
        var btn = e.target.closest('.btn-product-detail');
        if (!btn) return;

        var productId = btn.getAttribute('data-product-id');
        modalBody.innerHTML = '<div class="text-center text-muted py-4">Yükleniyor...</div>';
        modal.show();

        fetch('/Home/ProductDetails/' + productId)
            .then(function (res) {
                if (!res.ok) throw new Error('Ürün bilgisi alınamadı.');
                return res.text();
            })
            .then(function (html) {
                modalBody.innerHTML = html;
            })
            .catch(function () {
                modalBody.innerHTML = '<div class="alert alert-danger mb-0">Ürün bilgisi yüklenemedi.</div>';
            });
    });
})();
