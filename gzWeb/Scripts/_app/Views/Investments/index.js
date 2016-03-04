$(function () {
    var dataSource = new kendo.data.DataSource({
        dataType: "json",
        type: "GET",
        transport: {
            read: "/Investments/GetInvestAmnt"
        },            
        schema: {
            model: {
                fields: {
                    Amount: { type: "number" },
                    CreatedOnUTC: { type: "date" }
                }
            }
        },
        pageSize: 3,
        page: 1
    });
    $("#pager").kendoPager({
        dataSource: dataSource,
        pageSizes: false
    });
    $("#listView").kendoListView({
        dataSource: dataSource,
        template: '<tr><td>#:kendo.toString(CreatedOnUTC, "MMMM")#</td><td><span class="glyphicon-euro"> #:kendo.toString(Amount, "n2")#</span></td></tr>',
        //autoBind: true,
        pageable: true
    });
    //dataSource.read();
});