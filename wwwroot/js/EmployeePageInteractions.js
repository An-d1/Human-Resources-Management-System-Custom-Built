// Employees List Page JavaScript
(function () {
    'use strict';

    // Search functionality
    const searchInput = document.getElementById('searchInput');
    const employeeRows = document.querySelectorAll('.employee-row');

    if (searchInput) {
        searchInput.addEventListener('input', function () {
            const searchTerm = this.value.toLowerCase().trim();

            employeeRows.forEach(row => {
                const employeeId = row.querySelector('.employee-id')?.textContent.toLowerCase() || '';
                const employeeName = row.querySelector('.employee-name')?.textContent.toLowerCase() || '';
                const employeeEmail = row.querySelector('.employee-email')?.textContent.toLowerCase() || '';
                const employeeRole = row.querySelector('.role-badge')?.textContent.toLowerCase() || '';

                const matches = employeeId.includes(searchTerm) ||
                    employeeName.includes(searchTerm) ||
                    employeeEmail.includes(searchTerm) ||
                    employeeRole.includes(searchTerm);

                row.style.display = matches ? '' : 'none';
            });

            // Show empty state if no results
            updateEmptyState();
        });
    }

    // Select all checkbox functionality
    const selectAllCheckbox = document.getElementById('selectAll');
    const employeeCheckboxes = document.querySelectorAll('.employee-checkbox');

    if (selectAllCheckbox) {
        selectAllCheckbox.addEventListener('change', function () {
            employeeCheckboxes.forEach(checkbox => {
                if (checkbox.closest('.employee-row').style.display !== 'none') {
                    checkbox.checked = this.checked;
                }
            });
            updateBulkActions();
        });
    }

    // Individual checkbox changes
    employeeCheckboxes.forEach(checkbox => {
        checkbox.addEventListener('change', function () {
            updateSelectAllState();
            updateBulkActions();
        });
    });

    // Update select all checkbox state
    function updateSelectAllState() {
        if (!selectAllCheckbox) return;

        const visibleCheckboxes = Array.from(employeeCheckboxes).filter(cb =>
            cb.closest('.employee-row').style.display !== 'none'
        );

        const checkedCount = visibleCheckboxes.filter(cb => cb.checked).length;
        selectAllCheckbox.checked = checkedCount > 0 && checkedCount === visibleCheckboxes.length;
        selectAllCheckbox.indeterminate = checkedCount > 0 && checkedCount < visibleCheckboxes.length;
    }

    // Update bulk action buttons
    function updateBulkActions() {
        const checkedCount = Array.from(employeeCheckboxes).filter(cb => cb.checked).length;
        // You can add bulk action buttons here and show/hide based on checkedCount
    }