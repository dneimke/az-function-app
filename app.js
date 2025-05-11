document.addEventListener('DOMContentLoaded', () => {
    const callFunctionButton = document.getElementById('callFunction');
    const resultDiv = document.getElementById('result');

    callFunctionButton.addEventListener('click', async () => {
        // Update UI to show loading state
        callFunctionButton.disabled = true;
        resultDiv.innerHTML = '<p class="loading">Loading data from function...</p>';

        try {
            // Call the Azure Function
            const response = await fetch('http://localhost:7124/api/myfunction');

            if (!response.ok) {
                throw new Error(`Function returned an error: ${response.status} ${response.statusText}`);
            }

            // Parse the JSON response
            const data = await response.json();

            // Display the results
            let resultHtml = '<h3>Function Result:</h3>';

            if (Array.isArray(data)) {
                // Display array of items
                resultHtml += '<ul>';
                data.forEach(item => {
                    resultHtml += `<li>${JSON.stringify(item, null, 2)}</li>`;
                });
                resultHtml += '</ul>';
            } else {
                // Display single item or object
                resultHtml += `<pre>${JSON.stringify(data, null, 2)}</pre>`;
            }

            resultDiv.innerHTML = resultHtml;
        } catch (error) {
            // Handle any errors
            resultDiv.innerHTML = `<p class="error">Error: ${error.message}</p>`;
            console.error('Error calling function:', error);
        } finally {
            // Re-enable the button
            callFunctionButton.disabled = false;
        }
    });
});
