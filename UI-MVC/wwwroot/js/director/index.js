const tableBody = document.getElementById('directorsTableBody');
const refreshButton = document.getElementById('refreshButton');

async function loadDirectors() {
    try {
        const response = await fetch('/api/directors', {
            headers: {
                "Accept": "application/json"
            }
        });

        if (response.status === 200) {
            const data = await response.json();
            const directors = data.$values || data;

            tableBody.innerHTML = '';

            for (const director of directors) {
                const careerEnd = director.careerEnd ? director.careerEnd : 'Still active';

                tableBody.innerHTML += `
                    <tr>
                        <td>${director.name}</td>
                        <td>${director.country}</td>
                        <td>${director.careerStart || 'Unknown'}</td>
                        <td>${careerEnd}</td>
                        <td><a href="/Director/Details/${director.imdbId}">Details</a></td>
                    </tr>
                `;
            }
        } else if (response.status === 204) {
            tableBody.innerHTML = '<tr><td colspan="5" class="text-center">No directors found</td></tr>';
        } else {
            alert('Something went wrong loading directors...');
        }
    } catch (error) {
        console.error('Error loading directors:', error);
        alert('Failed to load directors. Please try again.');
    }
}

loadDirectors();

refreshButton.addEventListener('click', loadDirectors);