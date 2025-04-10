// **1. İsim ve Soyisim Kontrolü**
function validateName(name) {
    var regex = /^[a-zA-ZğüşöçıİĞÜŞÖÇ\s]+$/;
    return regex.test(name) && name.trim().length > 0;
}

// **2. Boş Alan Kontrolü**
function isEmpty(value) {
    return value.trim().length === 0;
}


// **3. Sayı Doğrulama (Örn: Ürün Fiyatı)**
function validateNumber(value) {
    return !isNaN(value) && value > 0;
}

// **5. Modal Açma**
function modalAc(baslik, icerikHtml, kaydetFonksiyonu) {
    $("#modalBaslik").text(baslik);
    $("#modalIcerik").html(icerikHtml);
    $("#modalKaydetButon").off("click").on("click", function () {
        kaydetFonksiyonu();
        $("#genelModal").hide();
    });
    $("#genelModal").show();
}

// **6. Form Resetleme**
function resetForm(formId) {
    $("#" + formId)[0].reset();
}

// **7. AJAX POST İşlemi**
function ajaxPost(url, data, successMessage, errorMessage) {
    $.post(url, data, function (response) {
        if (response == 200) {
            $.notify({ message: successMessage }, { type: "success", delay: 2000 });
        } else {
            $.notify({ message: errorMessage }, { type: "error", delay: 2000 });
        }
    });
}
