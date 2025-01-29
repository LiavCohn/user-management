const fs = require("fs");

function toPascalCase(str) {
  const exceptions = {
    userID: "UserID",
    userGroupID: "UserGroupID",
  };

  if (exceptions[str]) {
    return exceptions[str];
  }
  return str
    .match(/[A-Z]{2,}(?=[A-Z][a-z]+[0-9]*|\b)|[A-Z]?[a-z]+[0-9]*|[A-Z]|[0-9]+/g)
    .map((x) => x.charAt(0).toUpperCase() + x.slice(1).toLowerCase())
    .join("");
}

function normalizeKeys(obj) {
  const normalizedObj = {};
  for (const key in obj) {
    if (obj.hasOwnProperty(key)) {
      const pascalKey = toPascalCase(key);
      const value = obj[key];

      if (value && typeof value === "object" && !Array.isArray(value)) {
        normalizedObj[pascalKey] = normalizeKeys(value);
      } else {
        normalizedObj[pascalKey] = value;
      }
    }
  }
  return normalizedObj;
}

const file_location = "../Users.json";

const readUsers = () => {
  const data = fs.readFileSync(file_location);
  return JSON.parse(data).Users;
};

const writeUsers = (users) => {
  fs.writeFileSync(file_location, JSON.stringify({ Users: users }, null, 2));
};

module.exports = { normalizeKeys, readUsers, writeUsers };
