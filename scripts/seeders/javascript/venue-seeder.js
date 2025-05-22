const axios = require("axios");

const baseUrl = "http://localhost:8083";
const venueName = "Bernabeu Stadium";
const venueAddress = "Av. de Concha Espina, 1, Chamartín, 28036 Madrid, Spain";
const venueCity = "Madrid";

const userId = "00000000-0000-0000-0000-000000000001";
const userRoles = "Admin";
async function apiCall(method, endpoint, body = null) {
  const url = `${baseUrl}/${endpoint}`;
  console.log(`Making ${method} request to ${url}`);
  try {
    const response = await axios({
      method,
      url,
      data: body,
      headers: {
        "Content-Type": "application/json",
        Accept: "application/json",
        "X-User-Id": userId,
        "X-User-Roles": userRoles,
      },
    });
    return response.data;
  } catch (error) {
    console.error(`API call failed: ${error.message}`);
    if (error.response) {
      console.error(`Error: ${error.response.status}`);
    }
    throw error;
  }
}

async function createVenue(name, address, city) {
  try {
    const venue = await apiCall("post", "api/venues", {
      name,
      address,
      city,
      ownerUserId: userId,
    });
    console.log(`Created venue: ${venue.venueId}`);
    return venue;
  } catch (error) {
    console.error(`Failed to create venue: ${error.message}`);
    return null;
  }
}

async function createSectionWithSeats(venueId, name, rows, seatsPerRow) {
  const seats = [];
  for (let row = 0; row < rows; row++) {
    const rowLetter = String.fromCharCode("A".charCodeAt(0) + row);

    for (let seatNum = 1; seatNum <= seatsPerRow; seatNum++) {
      seats.push({
        seatNumber: `${seatNum}`,
        rowNumber: rowLetter,
        seatInRow: seatNum,
      });
    }
  }

  try {
    const section = await apiCall("post", `api/venues/${venueId}/sections`, {
      name,
      capacity: rows * seatsPerRow,
      seats,
    });
    return section;
  } catch (error) {
    console.error(`Failed to create section: ${error.message}`);
    return null;
  }
}

async function withRetry(fn, maxRetries = 3, delay = 1000) {
  let lastError;

  for (let attempt = 1; attempt <= maxRetries; attempt++) {
    try {
      return await fn();
    } catch (error) {
      lastError = error;
      console.error(
        `Attempt ${attempt}/${maxRetries} failed: ${error.message}`
      );

      if (attempt < maxRetries) {
        console.log(`Retrying in ${delay}ms...`);
        await new Promise((resolve) => setTimeout(resolve, delay));
        delay *= 2;
      }
    }
  }

  throw lastError;
}

async function seedVenue() {
  try {
    const venue = await withRetry(() =>
      createVenue(venueName, venueAddress, venueCity)
    );
    if (venue) {
      const BATCH_SIZE = 5;
      const totalSections = 20;
      for (
        let batchStart = 1;
        batchStart <= totalSections;
        batchStart += BATCH_SIZE
      ) {
        const batchEnd = Math.min(batchStart + BATCH_SIZE - 1, totalSections);
        const promises = [];
        for (
          let sectionNum = batchStart;
          sectionNum <= batchEnd;
          sectionNum++
        ) {
          const sectionName = `${sectionNum}`;
          promises.push(
            withRetry(() =>
              createSectionWithSeats(venue.venueId, sectionName, 14, 8)
            ).catch((error) => {
              console.error(
                `Failed to create section ${sectionName}: ${error.message}`
              );
              return null;
            })
          );
        }
        await Promise.all(promises);
      }
    } else {
      throw new Error("Failed to create venue");
    }

    console.log("Venue seeding completed successfully!");
  } catch (error) {
    console.error(
      `An error occurred during the seeding process: ${error.message}`
    );
    process.exit(1);
  }
}

seedVenue();
