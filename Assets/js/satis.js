let islemGecmisi = []; // Fatura çıkart'a basana kadar geçici olarak ürünleri tutar

function urunEkle() {
    let urunBarkod = $("#urunlerDropdownBtn").val();
    let adet = $("#urunAdet").val();
    let musteriID = $("#musterilerDropdownBtn").val();
    let calisanID = $("#calisanlarDropdownBtn").val();

    if (!calisanID || !urunBarkod || !adet || adet <= 0 || !musteriID) {
        $.notify({ message: "Lütfen tüm alanları eksiksiz ve doğru doldurun!", type: "danger", delay: 2000 });
        return;
    }

    if (/^0\d+/.test(adet) || adet.trim() === "") {
        $.notify({ message: "Lütfen başında sıfır olmayan geçerli bir tam sayı girin!", type: "danger", delay: 2000 });
        return;
    }

    if (!Number.isInteger(Number(adet))) {
        $.notify({ message: "Lütfen sadece tam sayı girin!", type: "danger", delay: 2000 });
        return;
    }

    // Ürün bilgilerini al (Adı ve fiyatı)
    $.get(`/Satis/urunBilgisi?urunBarkod=${urunBarkod}`)
        .done(function (res) {
            let yeniUrun = {
                urunBarkod: urunBarkod,
                urunAdi: res.urunAdi, // Ürün adı eklendi
                adet: Number(adet),
                musteriID: musteriID,
                calisanID: calisanID,
                fiyat: res.urunFiyati
            };

            islemGecmisi.push(yeniUrun);
            guncelleIslemGecmisi();

            $.notify({ message: `"${res.urunAdi}" ürünü sepete eklendi!`, type: "success", delay: 2000 });

            $("#urunlerDropdownBtn").val("");
            $("#urunAdet").val("");
        })
        .fail(function (jqXHR) {
            if (jqXHR.status === 400) {// 400 Bad Request
                $.notify({ message: `Ürün ${urunBarkod} için fiyat bulunamadı!`, type: "warning", delay: 3000 });
            } else {
                $.notify({ message: "Bağlantı hatası, lütfen tekrar deneyin!", type: "danger", delay: 2000 });
            }
        });
}



// İşlem geçmişini HTML'e yazdır
function guncelleIslemGecmisi() {
    let feed = $(".activity-feed");
    feed.empty();

    if (islemGecmisi.length === 0) {
        feed.append(`<li class='feed-item feed-item-warning'><span class='text'>Sepet boş.</span></li>`);
        return;
    } 

    //  İşlem tarihi (sadece gün)
    let islemTarihi = moment().format('YYYY-MM-DD');

    //  Tarihi "Tarih:" başlığının yanına yazalım
    $(".card-title:contains('Tarih:')").html(`Tarih: <strong>${islemTarihi}</strong>`);


    islemGecmisi.forEach((urun, index) => {
        feed.append(`
            <li class='feed-item feed-item-primary'>
                <span class='text'>
                    <strong>${urun.urunAdi}</strong> Barkod Numarası:${urun.urunBarkod}
                    <button onclick="urunuSepettenSil(${index})" type="button" class="btn btn-link btn-danger">
                        <i style="font-size: 19px;" class="fa fa-times"></i>
                    </button><br>
                    ${urun.adet} adet eklendi,
                    Birim Fiyat: ${urun.fiyat.toFixed(2)}₺,
                    Toplam Fiyat: ${(urun.fiyat * urun.adet).toFixed(2)}₺
                </span>
            </li>
        `);
    });
}


function urunuSepettenSil(index) {
    islemGecmisi.splice(index, 1); // Diziden kaldır
    guncelleIslemGecmisi(); // Arayüzü güncelle
    $.notify({ message: "Ürün sepetten çıkarıldı!", type: "warning", delay: 2000 });
}

//  Fatura oluştur - Sepetteki ürünleri veritabanına kaydet
function faturaOlustur() {
    if (islemGecmisi.length === 0) {
        $.notify({ message: "Sepet boş, fatura oluşturulamaz!", type: "danger", delay: 2000 });
        return;
    }

    let guncellenmisUrunler = [];

    let fiyatAlmaPromises = islemGecmisi.map(urun => {
        return $.get(`/Satis/urunBilgisi?urunBarkod=${urun.urunBarkod}`) // Artık urunFiyatlari yerine urunBilgisi kullanıyoruz!
            .then(res => {
                if (res.success) {
                    guncellenmisUrunler.push({
                        urunBarkod: urun.urunBarkod,
                        adet: urun.adet,
                        musteriID: urun.musteriID,
                        calisanID: urun.calisanID,
                        fiyat: res.urunFiyati
                    });
                } else {
                    $.notify({ message: `Ürün ${urun.urunBarkod} için fiyat bulunamadı!`, type: "warning", delay: 2000 });
                }
            });
    });

    Promise.all(fiyatAlmaPromises).then(() => {
        if (guncellenmisUrunler.length === 0) {
            $.notify({ message: "Hiçbir ürün için fiyat alınamadı, fatura oluşturulamaz!", type: "danger", delay: 2000 });
            return;
        }

        $.post("/Satis/faturaOlustur", { urunler: guncellenmisUrunler }, function (res) {
            if (res.success) {
                $.notify({ message: `Fatura oluşturuldu. Toplam: ${res.toplamFiyat.toFixed(2)}₺`, type: "success", delay: 3000 });

                // Fatura geçmişini güncelle
                setTimeout(() => faturaGecmisiniGetir(), 500);

                $("#calisanlarDropdownBtn, #musterilerDropdownBtn, #urunlerDropdownBtn").val("");
                $("#urunAdet").val("");
                islemGecmisi = [];
                $(".activity-feed").empty();
            } else if (res.message === "Stok yetersiz!") {
                res.stokHatasi.forEach(hata => {
                    $.notify({
                        message: `Stok yetersiz! Ürün ID: ${hata.urunID}, Stok: ${hata.stokMiktari}, İstenen: ${hata.istenenAdet}`,
                        type: "danger",
                        delay: 4000
                    });
                });
            } else {
                $.notify({ message: res.message, type: "danger", delay: 2000 });
            }
        }).fail(function () {
            $.notify({ message: "Bağlantı hatası, lütfen tekrar deneyin!", type: "danger", delay: 2000 });
        });
    });
}
// Formu temizle
function temizleForm() {
    $("#calisanlarDropdownBtn, #musterilerDropdownBtn, #urunlerDropdownBtn").val("");
    $("#urunAdet").val("");
    islemGecmisi = [];
    $(".activity-feed").empty();
    $.notify({ message: "Form ve işlem geçmişi temizlendi!", type: "info", delay: 2000 });
}

//  Satış geçmişini getir
function faturaGecmisiniGetir() {
    console.log("Fatura geçmişi güncelleniyor...");

    $.get("/Satis/faturaGecmisi", function (res) {
        console.log("Fatura geçmişi API Yanıtı:", res);

        let feed = $(".activity-feed");
        feed.empty();

        if (!res || res.length === 0) {
            console.warn("Fatura geçmişi API boş döndü.");
            feed.append(`<li class='feed-item feed-item-warning'><span class='text'>Henüz fatura oluşturulmadı.</span></li>`);
            return;
        }

        res.forEach(fatura => {
            feed.append(`
                <li class='feed-item feed-item-success'>
                    <time class='date'>${moment(fatura.faturaTarihi).format('YYYY-MM-DD HH:mm:ss')}</time>
                    <span class='text'>
                        <strong>Fatura ID:</strong> ${fatura.faturaID} <br>
                        <strong>Toplam Tutar:</strong> ${fatura.faturaTutari.toFixed(2)}₺
                    </span>
                </li>
            `);
        });
    }).fail(function () {
        $.notify({ message: "Fatura geçmişi yüklenirken hata oluştu!", type: "danger", delay: 2000 });
    });
}

// Sayfa yüklendiğinde fatura geçmişini getir
$(document).ready(function () {
    faturaGecmisiniGetir(); 
});
