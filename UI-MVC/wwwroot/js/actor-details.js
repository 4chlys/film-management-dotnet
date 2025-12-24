const actorIdInput = document.getElementById('actorId');
const filmsTableBody = document.getElementById('filmsTableBody');
const filmIdSelect = document.getElementById('filmId');
const screenTimeInput = document.getElementById('screenTime');
const addFilmButton = document.getElementById('addFilmButton');

const actorId = actorIdInput.value;

/**
 * Load all films for this actor
 */
async function loadActorFilms() {
    try {
        const response = await fetch(`/api/actors/${actorId}/films`, {
            headers: {
                "Accept": "application/json"
            }
        });

        if (response.status === 200) {
            const data = await response.json();
            const films = data.$values || data;

            filmsTableBody.innerHTML = '';

            for (const film of films) {
                const genreDisplay = film.genre || 'Unknown';

                const releaseDate = new Date(film.releaseDate);
                const releaseDateDisplay = releaseDate.toLocaleDateString('en-GB');

                const screenTimeDisplay = film.screenTime ? `${film.screenTime} min` : 'Unknown';

                filmsTableBody.innerHTML += `
                    <tr>
                        <td><a href="/Film/Details?id=${film.imdbId}">${film.title}</a></td>
                        <td>${genreDisplay}</td>
                        <td>${releaseDateDisplay}</td>
                        <td>${film.rating.toFixed(1)}</td>
                        <td>${screenTimeDisplay}</td>
                    </tr>
                `;
            }
        } else if (response.status === 204) {
            filmsTableBody.innerHTML = '<tr><td colspan="5" class="text-center">No films found for this actor</td></tr>';
        } else {
            console.error('Failed to load films:', response.status);
            alert('Failed to load films. Please try again.');
        }
    } catch (error) {
        console.error('Error loading actor films:', error);
        alert('An error occurred while loading films.');
    }
}

/**
 * Load all available films (excluding ones already linked to this actor)
 */
async function loadAvailableFilms() {
    try {
        const response = await fetch('/api/films', {
            headers: {
                "Accept": "application/json"
            }
        });

        if (response.status === 200) {
            const data = await response.json();
            const allFilms = data.$values || data;

            const actorFilmsResponse = await fetch(`/api/actors/${actorId}/films`, {
                headers: {
                    "Accept": "application/json"
                }
            });

            let linkedFilmIds = [];
            if (actorFilmsResponse.status === 200) {
                const actorFilmsData = await actorFilmsResponse.json();
                const actorFilms = actorFilmsData.$values || actorFilmsData;
                linkedFilmIds = actorFilms.map(f => f.imdbId);
            }

            const availableFilms = allFilms.filter(f => !linkedFilmIds.includes(f.imdbId));

            filmIdSelect.innerHTML = '<option value="">-- Select a film --</option>';

            for (const film of availableFilms) {
                filmIdSelect.innerHTML += `
                    <option value="${film.imdbId}">${film.title} (${new Date(film.releaseDate).getFullYear()})</option>
                `;
            }

            if (availableFilms.length === 0) {
                filmIdSelect.innerHTML = '<option value="">No films available to add</option>';
                addFilmButton.disabled = true;
            } else {
                addFilmButton.disabled = false;
            }
        }
    } catch (error) {
        console.error('Error loading available films:', error);
        alert('Failed to load available films.');
    }
}

/**
 * Add a film to this actor
 */
async function addFilmToActor() {
    const filmId = filmIdSelect.value;

    if (!filmId) {
        alert('Please select a film');
        return;
    }

    const screenTime = screenTimeInput.value ? parseInt(screenTimeInput.value) : null;

    try {
        const response = await fetch('/api/actorfilms', {
            method: "POST",
            headers: {
                "Accept": "application/json",
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                actorId: actorId,
                filmId: filmId,
                screenTime: screenTime
            })
        });

        if (response.status === 201) {
            await loadActorFilms();
            await loadAvailableFilms();

            screenTimeInput.value = '';
            filmIdSelect.value = '';

            alert('Film added successfully!');
        } else if (response.status === 400) {
            const error = await response.json();
            alert(`Failed to add film: ${error.error || 'Validation error'}`);
        } else {
            alert('Failed to add film. Please try again.');
        }
    } catch (error) {
        console.error('Error adding film:', error);
        alert('An error occurred while adding the film.');
    }
}

loadActorFilms();
loadAvailableFilms();

addFilmButton.addEventListener('click', addFilmToActor);