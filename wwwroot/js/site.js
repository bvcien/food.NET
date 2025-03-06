$(document).ready(function () {
    // Khi nhấn nút "-" để giảm số lượng
    $(".minus").click(function () {
        var quantityInput = $(this).siblings(".quantity"); // Lấy ô input số lượng
        var currentQuantity = parseInt(quantityInput.val()); // Lấy số lượng hiện tại

        // Nếu số lượng lớn hơn 1, giảm số lượng
        if (currentQuantity > 1) {
            quantityInput.val(currentQuantity - 1); // Cập nhật số lượng
            updateCart(quantityInput.data("id"), currentQuantity - 1); // Cập nhật giỏ hàng
        }
    });

    // Khi nhấn nút "+" để tăng số lượng
    $(".plus").click(function () {
        var quantityInput = $(this).siblings(".quantity"); // Lấy ô input số lượng
        var currentQuantity = parseInt(quantityInput.val()); // Lấy số lượng hiện tại
        quantityInput.val(currentQuantity + 1); // Cập nhật số lượng
        updateCart(quantityInput.data("id"), currentQuantity + 1); // Cập nhật giỏ hàng
    });

    // Hàm cập nhật giỏ hàng qua AJAX
    function updateCart(productId, quantity) {
        $.ajax({
            url: '@Url.Action("UpdateCart", "Hoaqua")', // Hành động UpdateCart
            type: 'POST',
            data: {
                id: productId,
                quantity: quantity
            },
            success: function (response) {
                console.log('Giỏ hàng đã được cập nhật');
                location.reload(); // Tải lại trang để cập nhật giỏ hàng
            },
            error: function (error) {
                console.error('Có lỗi xảy ra', error);
            }
        });
    }
});


