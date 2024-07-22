window.exportFunctions = {
    exportTableToPDF: function (selector, filename) {
        const element = document.querySelector(selector);
        if (element) {
            html2canvas(element).then(canvas => {
                const imgData = canvas.toDataURL('image/png');
                const pdf = new jsPDF();
                pdf.addImage(imgData, 'PNG', 0, 0);
                pdf.save(filename);
            });
        }
    },
    exportTableToExcel: function (tableId, filename) {
        const table = document.getElementById(tableId);
        if (table) {
            const wb = XLSX.utils.table_to_book(table, { sheet: "Sheet1" });
            XLSX.writeFile(wb, filename);
        }
    },
    printTable: function (tableId) {
        const table = document.getElementById(tableId);
        if (table) {
            const newWindow = window.open();
            newWindow.document.write(table.outerHTML);
            newWindow.print();
            newWindow.close();
        }
    }
};
