// Functionality to delete a row from the table
function deleteRow(button) {
    Swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete it!'
    }).then((result) => {
        if (result.isConfirmed) {
            var name = document.querySelector('form').getAttribute('data-spreadsheet-name'); // Get the spreadsheet name from the form attribute
            var spreadsheetId = document.querySelector('form').getAttribute('data-spreadsheet-id');
            var rowIndex = Array.prototype.indexOf.call(button.parentNode.parentNode.parentNode.children, button.parentNode.parentNode);
            fetch(`/api/GoogleSheetsAPI/delete/${spreadsheetId}/${rowIndex + 1}?spreadsheetName=${encodeURIComponent(name)}`, {
                    method: 'DELETE'
                })
                .then(response => {
                    if (response.ok) {
                        Swal.fire(
                            'Deleted!',
                            'Your row has been deleted.',
                            'success'
                        );
                        // Refresh the page to reflect the changes
                        location.reload();
                    } else {
                        throw new Error('Network response was not ok');
                    }
                })
                .catch(error => {
                    console.error('Error:', error);
                    Swal.fire(
                        'Error!',
                        'There was an error deleting your row.',
                        'error'
                    );
                });
        }
    });
}


// Functionality to add a new row to the table
document.getElementById('addRow').addEventListener('click', function () {
    var table = document.querySelector('table');
    var row = table.insertRow(-1); // Insert a new row at the end of the table

    // Add a checkbox cell to the new row
    var checkboxCell = row.insertCell(0);
    checkboxCell.className = "other-check";
    checkboxCell.innerHTML = '<input type="checkbox" class="row-select" data-row-index="new" />';

    for (var i = 0; i < table.rows[0].cells.length - 1; i++) { // Subtract 1 to account for the checkbox cell
        var cell = row.insertCell(i + 1); // Add 1 to account for the checkbox cell
        cell.innerHTML = '<input type="text" name="values[new][' + i + ']" value="" />'; // Add an input field to the cell
    }

    // Add a Delete button to the new row
    var deleteCell = row.insertCell(); // Insert a new cell at the end of the row
    deleteCell.innerHTML = '<button type="button" class="deleteRow" onclick="deleteRow(this)">Delete</button>'; // Add a Delete button to the cell
});



// Functionality to update the data in the Google Sheet
document.querySelector('form').addEventListener('submit', async function (event) {
    event.preventDefault();

    var tableRows = document.querySelectorAll('table tr');
    var data = Array.from(tableRows).slice(1).map(row => {
        return Array.from(row.querySelectorAll('input[type=text]')).map(input => input.value); // Only include text inputs
    });
    var spreadsheetId = this.getAttribute('data-spreadsheet-id'); // Get the spreadsheetId from the form attribute
    var name = this.getAttribute('data-spreadsheet-name'); // Get the spreadsheet name from the form attribute
    console.log('Spreadsheet ID:', spreadsheetId);
    console.log('Spreadsheet Name:', name);
    console.log(JSON.stringify({ values: data }));

    try {
        const response = await fetch(`/api/GoogleSheetsAPI/update/${spreadsheetId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ values: data, name: name }) // Include name in the request body
        });

        if (!response.ok) {
            const errorData = await response.json(); // Try to get more error info from the server
            console.error('Server responded with error:', errorData);
            throw new Error('Network response was not ok: ' + response.statusText);
        }

        let responseData;
        if (response.headers.get("Content-Type") === "application/json") {
            responseData = await response.json();
        } else {
            responseData = await response.text();
        }
        console.log(responseData); // Handle success

        // Display SweetAlert notification on success
        Swal.fire("Success!", "The data has been updated.", "success");

    } catch (error) {
        console.error('Error:', error); // Handle errors
        Swal.fire("Failed!", "There was a problem updating the data.", "error");
    }
});

//Functionality to draw a chart
// Load the Visualization API and the corechart package.
google.charts.load('current', { 'packages': ['corechart'] });

// Function to draw the chart
function drawChart(selectedRows) {
    var data = new google.visualization.DataTable();
    data.addColumn('string', 'Name');
    data.addColumn('number', 'Value');

    // Loop over each selected row index to build the chart data
    selectedRows.forEach(function (rowIndex) {
        var row = document.querySelectorAll('tbody tr')[rowIndex];
        var name = row.cells[1].querySelector('input').value; // Adjust the index as needed
        var value = parseFloat(row.cells[2].querySelector('input').value); // Adjust the index as needed

        // Check if the value is a number before adding it to the chart data
        if (!isNaN(value)) {
            data.addRow([name, value]);
        }
    });

    var options = {
        title: 'Your Chart Title',
        width: 400,
        height: 300
    };

    // Instantiate and draw the chart within the 'chart_div' element
    var chart = new google.visualization.ColumnChart(document.getElementById('chart_div'));
    chart.draw(data, options);
}

// Set a callback to run when the Google Visualization API is loaded.
google.charts.setOnLoadCallback(function () {
    drawChart([]); // Draw chart with no data initially
});

// Add event listener to the 'Draw Chart' button
document.getElementById('drawChartButton').addEventListener('click', function () {
    var selectedRows = Array.from(document.querySelectorAll('.row-select:checked'))
        .map(checkbox => checkbox.closest('tr'))
        .map(tr => Array.prototype.indexOf.call(tr.parentNode.children, tr));
    drawChart(selectedRows);
});

// Functionality to show/hide the chart
document.getElementById('toggleButton').addEventListener('click', function () {
    var contentContainer = document.querySelector('.content-container');
    var showHideButton = document.getElementById('toggleButton');
    if (contentContainer.style.display === 'none') {
        contentContainer.style.display = 'block';
        showHideButton.textContent = 'Hide Chart';
    } else {
        contentContainer.style.display = 'none';
        showHideButton.textContent = 'Show Chart';
    }
});