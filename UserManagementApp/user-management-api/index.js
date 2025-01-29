const express = require("express");
const bodyParser = require("body-parser");
const app = express();
const port = 3000;
const { normalizeKeys, readUsers, writeUsers } = require("./utils");

const jwt = require("jsonwebtoken");
const TOKEN_SECRET = "supersecretkey";
const TOKEN_EXPIRATION = "1h";
app.use(bodyParser.json());

app.post("/auth/token", (req, res) => {
  const whitelist = ["frontend-client"];
  const { clientId } = req.body;
  if (!whitelist.includes(clientId)) {
    return res.status(401).json({ message: "Unauthorized" });
  }

  const token = jwt.sign({ clientId }, TOKEN_SECRET, {
    expiresIn: TOKEN_EXPIRATION,
  });
  res.json({ token });
});

// middleware
const authenticateToken = (req, res, next) => {
  const authHeader = req.headers["authorization"];
  const token = authHeader && authHeader.split(" ")[1]; //Bearer xxxx

  if (!token) {
    return res.status(401).json({ message: "Unauthorized" });
  }

  jwt.verify(token, TOKEN_SECRET, (err, user) => {
    if (err) {
      return res.status(403).json({ message: "Forbidden" });
    }
    next();
  });
};

// get all users with optional filtering
app.get("/users", authenticateToken, (req, res) => {
  const users = readUsers();
  const { status, search } = req.query;

  let filteredUsers = users;

  // filter by active status
  if (status) {
    const isActive = status.toLowerCase() === "active";
    filteredUsers = filteredUsers.filter((user) => user.Active === isActive);
  }

  // filter by search query
  if (search) {
    filteredUsers = filteredUsers.filter(
      (user) =>
        user.UserName.includes(search) ||
        user.Data.Email.includes(search) ||
        user.Data.Phone.includes(search)
    );
  }

  res.json(filteredUsers);
});

// get user by id
app.get("/users/:id", authenticateToken, (req, res) => {
  const users = readUsers();
  const user = users.find((u) => u.UserID === parseInt(req.params.id));
  if (user) {
    res.json(user);
  } else {
    res.status(404).json({ message: "User not found" });
  }
});

// create a new user
app.post("/users/create", authenticateToken, (req, res) => {
  const users = readUsers();
  var newUser = req.body;
  newUser = normalizeKeys(newUser);
  // check if username exists
  const usernameExists = users.some((u) => u.UserName === newUser.UserName);
  if (usernameExists) {
    return res.status(400).json("Username already exists");
  }

  newUser.UserID = users.length + 1;
  newUser.Data.CreateionDate = new Date()
    .toISOString()
    .split("T")[0]
    .toString();
  users.push(newUser);

  // Write updated users back to the file
  writeUsers(users);
  res.status(201).json(newUser);
});

// update a user
app.put("/users/:id", authenticateToken, (req, res) => {
  const users = readUsers();
  const userIndex = users.findIndex(
    (u) => u.UserID === parseInt(req.params.id)
  );

  if (userIndex === -1) {
    return res.status(404).json("User not found");
  }
  var newUser = req.body;

  newUser = normalizeKeys(newUser);
  users[userIndex] = { ...users[userIndex], ...newUser };

  writeUsers(users);
  res.json(users[userIndex]);
});

// delete a user
app.delete("/users/:id", authenticateToken, (req, res) => {
  const users = readUsers();
  const userIndex = users.findIndex(
    (u) => u.UserID === parseInt(req.params.id)
  );

  if (userIndex === -1) {
    return res.status(404).json("User not found");
  }

  users.splice(userIndex, 1);

  writeUsers(users);
  res.status(204).send();
});

app.listen(port, () => {
  console.log(`API server running at http://localhost:${port}`);
});
