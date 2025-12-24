const tableBody = document.getElementById('actorsTableBody');
const refreshButton = document.getElementById('refreshButton');

async function loadActors() {
    try {
        const response = await fetch('/api/actors', {
            headers: {
                "Accept": "application/json"
            }
        });

        if (response.status === 200) {
            const data = await response.json();
            const actors = data.$values || data;

            tableBody.innerHTML = '';

            for (const actor of actors) {
                const dateOfBirth = new Date(actor.dateOfBirth);
                const dateOfBirthDisplay = dateOfBirth.toLocaleDateString('en-GB');

                const dateOfDeath = actor.dateOfDeath
                    ? new Date(actor.dateOfDeath).toLocaleDateString('en-GB')
                    : 'Still alive';

                tableBody.innerHTML += `
                    <tr>
                        <td>${actor.name}</td>
                        <td>${actor.nationality}</td>
                        <td>${dateOfBirthDisplay}</td>
                        <td>${dateOfDeath}</td>
                        <td>${actor.age}</td>
                        <td><a href="/Actor/Details?id=${actor.imdbId}">Details</a></td>
                    </tr>
                `;
            }
        } else if (response.status === 204) {
            tableBody.innerHTML = '<tr><td colspan="6" class="text-center">No actors found</td></tr>';
        } else {
            alert('Something went wrong loading actors...');
        }
    } catch (error) {
        console.error('Error loading actors:', error);
        alert('Failed to load actors. Please try again.');
    }
}

loadActors();

refreshButton.addEventListener('click', loadActors);