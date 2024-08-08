
/// all products page

document.addEventListener('DOMContentLoaded', async () => {
    if (window.location.pathname.includes('AllProducts.html')) {
        const unitsContainer = document.getElementById('units-container');

        try {
            const response = await fetch('https://localhost:7036/api/Platform/GetAllUnitsWithPages');
            if (!response.ok) {
                throw new Error('Failed to fetch units and pages');
            }

            const unitsWithPages = await response.json();

            let currentRow = null;

            unitsWithPages.forEach((unit, index) => {
                if (index % 2 === 0) {
                    currentRow = document.createElement('div');
                    currentRow.classList.add('row', 'mb-4');
                    unitsContainer.appendChild(currentRow);
                }

                const unitDiv = document.createElement('div');
                unitDiv.classList.add('col-md-5', 'mx-auto');

                const card = document.createElement('div');
                card.classList.add('card');

                // Upper part: Title and Dropdown
                const upperPart = createUpperPart(unit);
                card.appendChild(upperPart);

                // Middle part: Iframe
                const middlePart = createMiddlePart(unit);
                card.appendChild(middlePart);

                // Bottom part: Update Date
                const bottomPart = createBottomPart(unit);
                card.appendChild(bottomPart);

                unitDiv.appendChild(card);
                currentRow.appendChild(unitDiv);
            });
        } catch (error) {
            console.error('Error fetching units and pages:', error);
        }
    }
});


    function createUpperPart(unit) {
        const upperPart = document.createElement('div');
        upperPart.classList.add('card-header');

        const titleWithDropdown = document.createElement('div');
        titleWithDropdown.classList.add('d-flex', 'justify-content-between', 'align-items-center');

        const title = document.createElement('h5');
        title.textContent = unit.unit_name;
        titleWithDropdown.appendChild(title);

        const dropdown = createDropdown(unit);
        titleWithDropdown.appendChild(dropdown);
        upperPart.appendChild(titleWithDropdown);

        return upperPart;
    }



    function createMiddlePart(unit) {
        const middlePart = document.createElement('div');
        middlePart.classList.add('card-middle');  // Changed from 'card-body' to 'card-middle'

        // Create an orange line at the top of the middle part
        const topLine = document.createElement('div');
        topLine.classList.add('top-orange-line');
        middlePart.appendChild(topLine);

        // Display the unit name
        const unitNameElement = document.createElement('h5');
        unitNameElement.textContent = unit.unit_name;
        unitNameElement.classList.add('unit-name');
        middlePart.appendChild(unitNameElement);

        // Create an orange line at the bottom of the middle part
        const bottomLine = document.createElement('div');
        bottomLine.classList.add('bottom-orange-line');
        middlePart.appendChild(bottomLine);

        return middlePart;
    }



    function createBottomPart(unit) {
        const bottomPart = document.createElement('div');
        bottomPart.classList.add('rm-bg', 'card-footer', 'text-muted', 'd-flex', 'flex-column', 'align-items-center');

        // Check if pages are available and display the last update date
        if (unit.pages && unit.pages.length > 0) {
            const firstPage = unit.pages[0];
            const lastUpdate = document.createElement('p');
            lastUpdate.textContent = `תאריך עדכון אחרון: ${new Date(firstPage.update_Date).toLocaleDateString('he-IL')}`;
            lastUpdate.style.width = '100%'; // Ensure full width to center text
            lastUpdate.style.textAlign = 'center'; // Center the text
            bottomPart.appendChild(lastUpdate);
        }

        // Creating the enter button
        const enterButton = document.createElement('button');
        enterButton.classList.add('custom-enter-button');  // Refer to the new custom class for styling
        enterButton.textContent = 'כניסה ללומדה';
        enterButton.onclick = function () {
        //    window.location.href = `ViewPage.html?unitid=${unit.unit_id}`;
            window.location.href = `ViewPage.html?unitid=${unit.unit_id}&pageid=${unit.pages[0].page_id}`;  // Redirect to unit's main page with the first page ID
        };
        bottomPart.appendChild(enterButton);

        return bottomPart;
    }




    function createDropdown(unit) {
        const dropdown = document.createElement('div');
        dropdown.classList.add('dropdown');

        const dropdownButton = document.createElement('button');
        dropdownButton.classList.add('btn', 'btn-secondary', 'dropdown-toggle');
        dropdownButton.setAttribute('data-bs-toggle', 'dropdown');
        dropdownButton.textContent = '...';
        dropdown.appendChild(dropdownButton);

        const dropdownMenu = document.createElement('ul');
        dropdownMenu.classList.add('dropdown-menu');

        // Define options and their actions
        const options = {
            'עריכת תוכן': () => window.location.href = `PagesOfUnit.html?unit_id=${unit.unit_id}`,
            // Redirect to PagesOfUnit.html with unit_id
            'הגדרות הלומדה': () => {
                console.log('הגדרות הלומדה button clicked');
                fetchUnitSettings(unit.unit_id, saveChanges); // Fetch unit settings and pass the saveChanges function as a callback
            },
            'מחיקת לומדה': () => {
                // Show the confirmation modal
                $('#deleteConfirmationModal').modal('show');
                // Add event listener to the confirmation button
                const confirmDeleteButton = document.getElementById('confirmDeleteButton');
                confirmDeleteButton.addEventListener('click', async () => {
                    try {
                        console.log("sent to delete func");

                        // Perform deletion here
                        await deleteUnit(unit.unit_id);
                        console.log("unit sent to func"+unit.unit_id);
                        // Close the modal after deletion
                        $('#deleteConfirmationModal').modal('hide');

                    } catch (error) {
                        console.error('Error deleting unit:', error);
                        // Handle error
                    }
                });
            }
        };
        Object.entries(options).forEach(([label, action]) => {
            const item = document.createElement('li');
            const link = document.createElement('a');
            link.classList.add('dropdown-item');
            link.textContent = label;
            link.href = '#';
            link.addEventListener('click', (e) => {
                e.preventDefault(); // Prevent default link behavior
                action(); // Execute the corresponding action
            });
            item.appendChild(link);
            dropdownMenu.appendChild(item);
        });

        dropdown.appendChild(dropdownMenu);
        return dropdown;
    }










//delete modal

    async function deleteUnit(idToDelete) {
        const url = `/api/Platform/DeleteUnit/${idToDelete}`;

        console.log(`Deleting unit with ID: ${idToDelete}`);
        console.log(`Request URL: ${url}`);

        try {
            const response = await fetch(url, {
                method: 'DELETE',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            const successMessage = document.querySelector('.unit-success-message-delete');
            const errorMessage = document.querySelector('.unit-error-message-delete');
            const deleteConfirmationModal = $('#deleteConfirmationModal');

            if (response.ok) {
                console.log('Unit and associated pages deleted successfully');

                if (successMessage) {
                    successMessage.style.display = 'block';
                }

                // Approach the card itself
                const unitCard = document.querySelector(`.card[data-unit-id="${idToDelete}"]`);

                if (unitCard) {
                    unitCard.remove(); // Remove the card from the DOM
                } else {
                    console.error(`Unit card with ID ${idToDelete} not found`);
                }

                // Close the modal after displaying the success message
                setTimeout(() => {
                    if (successMessage) {
                        successMessage.style.display = 'none';
                    }
                    if (deleteConfirmationModal) {
                        deleteConfirmationModal.modal('hide');
                    }
                }, 3000); // Adjust the timeout duration as needed
                location.reload();

            } else {
                const errorText = await response.text();
                console.error('Failed to delete unit:', errorText);

                if (errorMessage) {
                    errorMessage.style.display = 'block';
                }

                // Hide success message if previously displayed
                if (successMessage) {
                    successMessage.style.display = 'none';
                }
            }

        } catch (error) {
            console.error('Error occurred while deleting unit:', error);

            // Display error message
            if (errorMessage) {
                errorMessage.style.display = 'block';
            }

            // Hide success message if previously displayed
            if (successMessage) {
                successMessage.style.display = 'none';
            }
        }
    }
















    //// In unit settings data - the correct func

async function fetchUnitSettings(unitId) {
    if (unitId === 1) {
        console.log("Editing is disabled for unit 1.");
        alert("אין אפשרות לערוך את יחידת המקור"); // Display an alert message

        return; // Exit the function if the unitId is 1
    }
        try {
            console.log("the unit id"+unitId);
            const response = await fetch(`/api/Platform/GetUnitSettings?unitid=${unitId}`);
            if (!response.ok) {
                throw new Error('Failed to fetch unit settings: ' + response.statusText);
            }

            const unitData = await response.json();
            console.log('Unit settings data:', unitData);

            if (unitData.length > 0) {
                // Populate modal fields with fetched data
                document.getElementById('inputTitle').value = unitData[0].unit_name;
                document.getElementById('inputDescription').value = unitData[0].unit_Description;
                document.getElementById('selectStatus').value = unitData[0].category_name;
               // document.getElementById('checkActive').checked = unitData[0].is_Published === 'Yes';

                // Open the modal
                console.log('Attempting to open modal...');
                var settingsModal = new bootstrap.Modal(document.getElementById('settingsModal'));
                settingsModal.show();

                // Attach event listener to "Save changes" button
                document.getElementById('saveChangesButton').addEventListener('click', async () => {
                    console.log('Save changes button clicked');
                    saveChanges(unitId, unitData); // Pass unitData[0] to saveChanges
                });

            } else {
                console.log('No unit settings found for the specified unit ID.');
            }

        } catch (error) {
            console.error('Error fetching unit settings:', error);
            // Handle the error here, e.g., display an error message to the user
        }
    }







    //change unit settings 

    async function saveChanges(unitId) {
        try {
            console.log(unitId);
            const unitName = document.getElementById('inputTitle').value;
            const unitDescription = document.getElementById('inputDescription').value;
            const categoryName = document.getElementById('selectStatus').value;
            //const isPublished = document.getElementById('checkActive').checked;

            const url = '/api/Platform/UpdateUnit';

            const requestData = {
                unit_id: unitId,
                unit_name: unitName,
                Unit_Description: unitDescription,
                category_name: categoryName,
                //is_Published: isPublished
            };

      
            console.log('Request data being sent:', requestData);

            const response = await fetch(url, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(requestData)
            });

            if (!response.ok) {
                let errorMessage = 'Failed to update unit settings.';

                if (response.status === 400) {
                    errorMessage += ' Bad Request: Please check the data you entered.';
                } else {
                    errorMessage += ` ${response.statusText}`;
                }
                throw new Error(errorMessage);
            }

            const updatedUnit = await response.json();
            const successMessage = document.querySelector('.unit-success-message');

            if (successMessage) {
                successMessage.style.display = 'block';

                // Hide the success message after a delay
                setTimeout(() => {
                    successMessage.style.display = 'none';
                }, 2500);
                location.reload();

            }

            // Hide the error message if previously displayed
            const errorMessage = document.querySelector('.unit-error-message');
            if (errorMessage) {
                errorMessage.style.display = 'none';
            }

        } catch (error) {
            console.error('Error updating unit settings:', error);
            // Display error message
            const errorMessage = document.querySelector('.unit-error-message');
            if (errorMessage) {
                errorMessage.style.display = 'block';
            }

            // Hide success message if previously displayed
            const successMessage = document.querySelector('.unit-success-message');
            if (successMessage) {
                successMessage.style.display = 'none';
            }
        }
    }







//VIEW PAGE




//document.addEventListener('DOMContentLoaded', () => {
//    if (window.location.pathname.includes('ViewPage.html')) {

//        const queryParams = new URLSearchParams(window.location.search);
//        const unitId = queryParams.get('unitid');
//        if (unitId) {
//            fetchPagesForUnit(unitId);
//        } else {
//            document.querySelector('.unit-container').textContent = 'No unit specified.';
//        }
//    }
//    // Add event listener to the share button
//    const shareButton = document.getElementById('shareButton');
//    if (shareButton) {
//        shareButton.addEventListener('click', sharePageUrl);
//    }
//});

//function sharePageUrl() {
//    const shareData = {
//        title: 'לצפייה בלומדה',
//        text: 'הנה קישור לצפייה בלומדה ה',
//        url: window.location.href
//    };

//    if (navigator.share) {
//        navigator.share(shareData)
//            .then(() => console.log('Page shared successfully'))
//            .catch(error => console.error('Error sharing the page:', error));
//    } else {
//        showModal(shareData);
//    }
//}


/////try it for media
//function showModal(shareData) {
//    const modal = document.getElementById('shareModal'); // Make sure this matches your modal's ID in HTML
//    const closeButton = document.querySelector("[data-bs-dismiss='modal']"); // Better way to reference the close button if using Bootstrap
//    document.getElementById('shareUrl').value = shareData.url;

//    var myModal = new bootstrap.Modal(modal); // Using Bootstrap's modal methods to handle the modal
//    myModal.show();

//    closeButton.onclick = function () {
//        myModal.hide(); // Using Bootstrap's methods to hide the modal
//    }

//    window.onclick = function (event) {
//        if (event.target == modal) {
//            myModal.hide(); // Using Bootstrap's methods to hide the modal
//        }
//    }
//}

//function copyUrl() {
//    const copyText = document.getElementById("shareUrl");
//    copyText.select();
//    copyText.setSelectionRange(0, 99999);
//    document.execCommand("copy");
////    alert("הURL הועתק: " + copyText.value);
//}







//async function fetchPagesForUnit(unitId) {
//    console.log("Starting to fetch pages for unit ID:", unitId);
//    const url = `https://localhost:7036/api/Platform/GetPagesByUnitId?unitId=${unitId}`;
//    try {
//        const response = await fetch(url);
//        if (!response.ok) {
//            throw new Error('Failed to fetch pages');
//        }
//        const pages = await response.json();
//        console.log("Pages retrieved for unit ID " + unitId + ":", pages);
//        if (pages.length > 0) {
//            displayPage(pages[0]);  // Assuming you want to display the first page only
//        } else {
//            document.querySelector('.unit-container').textContent = 'No pages available for this unit.';
//        }
//    } catch (error) {
//        console.error('Error fetching pages for unit ID ' + unitId + ':', error);
//        document.querySelector('.unit-container').textContent = 'Failed to load page content.';
//    }
//}

//function displayPage(page) {
//    const pageContentContainer = document.querySelector('.unit-container');
//    pageContentContainer.innerHTML = `
//        <p>${page.page_Content}</p>
//    `;
//}




document.addEventListener('DOMContentLoaded', () => {
    if (window.location.pathname.includes('ViewPage.html')) {
        const queryParams = new URLSearchParams(window.location.search);
        const unitId = queryParams.get('unitid');
        const pageId = queryParams.get('pageid');

        console.log("Page ID:", pageId, "Unit ID:", unitId); // Debugging log

      
    }
    // Add event listener to the share button
    const shareButton = document.getElementById('shareButton');
    if (shareButton) {
        shareButton.addEventListener('click', sharePageUrl);
    }
});

function sharePageUrl() {
    const shareData = {
        title: 'לצפייה בלומדה',
        text: 'הנה קישור לצפייה בלומדה ה',
        url: window.location.href
    };

    if (navigator.share) {
        navigator.share(shareData)
            .then(() => console.log('Page shared successfully'))
            .catch(error => console.error('Error sharing the page:', error));
    } else {
        showModal(shareData);
    }
}

function showModal(shareData) {
    const modal = document.getElementById('shareModal'); // Make sure this matches your modal's ID in HTML
    const closeButton = document.querySelector("[data-bs-dismiss='modal']"); // Better way to reference the close button if using Bootstrap
    document.getElementById('shareUrl').value = shareData.url;

    var myModal = new bootstrap.Modal(modal); // Using Bootstrap's modal methods to handle the modal
    myModal.show();

    closeButton.onclick = function () {
        myModal.hide(); // Using Bootstrap's methods to hide the modal
    }

    window.onclick = function (event) {
        if (event.target == modal) {
            myModal.hide(); // Using Bootstrap's methods to hide the modal
        }
    }
}

function copyUrl() {
    const copyText = document.getElementById("shareUrl");
    copyText.select();
    copyText.setSelectionRange(0, 99999);
    document.execCommand("copy");
    //    alert("הURL הועתק: " + copyText.value);
}



//async function fetchPageContent(unitId, pageId) {
//    console.log("Starting to fetch page content for unit ID:", unitId, "and page ID:", pageId);
//    const url = `https://localhost:7036/api/Platform/GetPageContent?unitId=${unitId}&pageId=${pageId}`;
//    try {
//        const response = await fetch(url);
//        if (!response.ok) {
//            throw new Error('Failed to fetch page content');
//        }
//        const pageContent = await response.json();
//        console.log("Page content retrieved for unit ID " + unitId + " and page ID " + pageId + ":", pageContent);
//        displayPage(pageContent);
//    } catch (error) {
//        console.error('Error fetching page content for unit ID ' + unitId + ' and page ID ' + pageId + ':', error);
//        document.querySelector('.unit-container').textContent = 'Failed to load page content.';
//    }
//}

function displayPage(page) {
    const pageContentContainer = document.querySelector('.unit-container');
    pageContentContainer.innerHTML = `
        <p>${page.page_Content}</p>
    `;
}







//HOMEPAGE
document.addEventListener('DOMContentLoaded', function () {
    if (window.location.pathname.includes('HomePage.html')) {
        fetchRecentlyEditedUnits();
    } else {
        console.log('Current page is not HomePage.html. No action taken.');
    }
});


function fetchRecentlyEditedUnits() {
    fetch('https://localhost:7036/api/Platform/GetAllUnitsWithPages')  // Updated endpoint
        .then(response => {
            if (!response.ok) {
                console.error('Failed to fetch data. Response status:', response.status);
                throw new Error('Failed to fetch data');
            }
            return response.json();
        })
        .then(data => {
            if (Array.isArray(data)) {
                populateGallery(data);
            } else {
                console.error('Data received is not an array:', data);
            }
        })
        .catch(error => console.error('Error loading recently edited units:', error));
}

function populateGallery(units) {
    const galleryContainer = document.querySelector('.gallery');
    if (!galleryContainer) {
        console.error('Gallery container not found.');
        return;
    }

    galleryContainer.innerHTML = '';

    console.log(units);
    // need to change unit.unitupdatedate to page.updatedates
    units.forEach(unit => {
        let latestUpdateDate = unit.unit_UpdateDate ? new Date(unit.unit_UpdateDate).toLocaleDateString('he-IL') : 'N/A';
        let cardDiv = document.createElement('div');
        cardDiv.className = 'gallery-item';
        cardDiv.innerHTML = `
            <div class="card">
                <div class="card-header">
                    <!-- Additional header content if needed -->
                </div>
                <div class="card-middle">
                    <div class="top-orange-line"></div>
                    <h5 class="unit-name">${unit.unit_name}</h5>
                    <div class="bottom-orange-line"></div>
                </div>
                <div class="rm-bg card-footer text-muted d-flex flex-column align-items-center">
                    <p style="width: 100%; text-align: center;">תאריך עדכון אחרון: ${latestUpdateDate}</p>
                                    <button class="custom-enter-button" onclick="window.location.href='ViewPage.html?unitid=${unit.unit_id}&pageid=${unit.pages[0].page_id}'">כניסה ללומדה</button>

                </div>
            </div>
        `;
        galleryContainer.appendChild(cardDiv);
    });

    const prevButton = document.querySelector('.gallery-prev');
    const nextButton = document.querySelector('.gallery-next');
    const items = document.querySelectorAll('.gallery-item');
    let currentIndex = 0;
    const itemsToShow = 3; // Number of items to show at a time

    const updateGallery = () => {
        items.forEach((item, index) => {
            if (index >= currentIndex && index < currentIndex + itemsToShow) {
                item.classList.add('active');
            } else {
                item.classList.remove('active');
            }
        });

        // Enable or disable navigation buttons based on currentIndex
        prevButton.disabled = currentIndex === 0;
        nextButton.disabled = currentIndex >= items.length - itemsToShow;
    };



    const prevClickHandler = () => {
        console.log('Previous button clicked'); // Debugging log
        if (currentIndex > 0) {
            currentIndex--;
            updateGallery();
        }
    };

    const nextClickHandler = () => {
        console.log('Next button clicked'); // Debugging log
        if (currentIndex < items.length - itemsToShow) {
            currentIndex++;
            updateGallery();
        }
    };

    // Remove existing event listeners before adding new ones
    prevButton.removeEventListener('click', prevClickHandler);
    nextButton.removeEventListener('click', nextClickHandler);

    // Add new event listeners
    prevButton.addEventListener('click', prevClickHandler);
    nextButton.addEventListener('click', nextClickHandler);

    updateGallery();
}


///LogIn




document.addEventListener('DOMContentLoaded', function () {
    if (window.location.pathname.includes('logIn.html')) {
        document.getElementById('loginForm').addEventListener('submit', function (event) {
            event.preventDefault(); // Prevent the default form submission
            const username = document.getElementById('username').value;
            const password = document.getElementById('password').value;

            if (username === 'admin' && password === 'admin123') {
                console.log("Logged in successfully");
                // Store login time and set session expiration
                localStorage.setItem('loggedIn', 'true');
                localStorage.setItem('loginTime', new Date().getTime());
                document.getElementById('successMessage').style.display = 'block';
                document.getElementById('successMessage').innerText = 'ברוך הבא admin, הינך מועבר למערכת';
                // Delay the redirection by 2 seconds
                setTimeout(function () {
                    window.location.href = 'HomePage.html';
                }, 3000);
            } else {
                console.log("Login failed");
                const errorMessage = document.getElementById('errorMessage');
                errorMessage.style.display = 'block';
                errorMessage.innerText = 'שם משתמש או סיסמה לא נכונים';
                // Hide the error message after 5 seconds
                setTimeout(function () {
                    errorMessage.style.display = 'none';
                }, 3000);
            }
        });
    }

    // Add event listener for logout link
    const logoutLink = document.getElementById('logoutLink');
    if (logoutLink) {
        logoutLink.addEventListener('click', function (event) {
            event.preventDefault();
            // Clear any session or localStorage data if needed
            localStorage.removeItem('loggedIn');
            localStorage.removeItem('loginTime');
            // Redirect to logIn.html
            window.location.href = 'logIn.html';
        });
    }
});

