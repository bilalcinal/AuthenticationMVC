window.addEventListener('DOMContentLoaded', event => {
    // Simple-DataTables
    // https://github.com/fiduswriter/Simple-DataTables/wiki

    const datatablesSimple = document.getElementById('datatablesSimple');
    if (datatablesSimple) {
        new simpleDatatables.DataTable(datatablesSimple);
    }
});
if ($('.table-shorting').length > 0) {
    $(".table-shorting tbody").sortable({
        cursor: "move",
        placeholder: "sortable-placeholder",
        helper: function (e, tr) {
            var $originals = tr.children();
            var $helper = tr.clone();
            console.log(tr.children());
            $helper.children().each(function (index) {
                $(this).width($originals.eq(index).width());
            });
            return $helper;
        }
    }).disableSelection();
    $(document).on("click", ".orderBtn", function () {
        var orderArray = [];
        const urlOrder = `/Cms/${$(this).attr("data-controler")}/${$(this).attr("data-action")}`;
        let tBody = $('.table-shorting').children("tbody");
        for (var i = 0; i < $(tBody).children("tr").length; i++) {
            orderArray.push(new row($(tBody).children("tr")[i].getAttribute("data-id"), i));
        }
        $('.loa').show();
        $.ajax({
            url: urlOrder,
            type: "POST",
            data: { orders: orderArray },
            datatype: "json",
            success: function (res) {
                if (res.status == 200) {
                    toastr.success(`${res.message}`);
                    setTimeout(function () {
                        window.location.reload();
                        $('.loa').hide();
                    }, 1500)
                }
            }
        });
        function row(id, place) {
            this.Id = id,
                this.Place = place
        }
    })
}

