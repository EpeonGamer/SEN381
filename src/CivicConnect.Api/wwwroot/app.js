// CivicConnect frontend. Talks only to the real API; the API enforces every rule,
// so hiding a button here is a convenience, never a security control.
// User-supplied text is always written with textContent (never innerHTML) to prevent script injection.

const $ = (id) => document.getElementById(id);
let currentUser = null;
let openRequestId = null;

// ---- API helper -------------------------------------------------------------------------------
async function api(method, path, body) {
  const response = await fetch(path, {
    method,
    headers: body ? { "Content-Type": "application/json" } : {},
    body: body ? JSON.stringify(body) : undefined,
  });
  const data = response.status === 204 ? null : await response.json().catch(() => null);
  if (!response.ok) {
    throw new Error(data?.detail || data?.error || `Request failed (${response.status})`);
  }
  return data;
}

function show(text, isError = false) {
  $("message").textContent = text;
  $("message").className = isError ? "error" : "ok";
}

// ---- Login / logout ---------------------------------------------------------------------------
$("login-form").addEventListener("submit", async (event) => {
  event.preventDefault();
  const form = new FormData(event.target);
  try {
    currentUser = await api("POST", "/api/auth/login", {
      userId: form.get("userId"), password: form.get("password"),
    });
    event.target.reset();
    show("");
    await startSession();
  } catch (error) {
    show("Login failed: " + error.message, true);
  }
});

$("logout").addEventListener("click", async () => {
  await api("POST", "/api/auth/logout");
  location.reload();
});

async function startSession() {
  $("who").textContent = currentUser.id;
  $("role").textContent = currentUser.role;
  $("session").hidden = false;
  $("login-view").hidden = true;
  $("list-view").hidden = false;
  $("submit-view").hidden = currentUser.role !== "Requester";
  $("counts-view").hidden = currentUser.role !== "Manager";
  $("list-title").textContent = currentUser.role === "Requester" ? "My requests" : "Requests";

  if (currentUser.role === "Requester") await loadCategories();
  await refresh();
}

// ---- Requester: submit ------------------------------------------------------------------------
async function loadCategories() {
  const select = $("category");
  select.replaceChildren();
  for (const name of await api("GET", "/api/categories")) {
    select.append(new Option(name, name));
  }
}

$("submit-form").addEventListener("submit", async (event) => {
  event.preventDefault();
  const form = new FormData(event.target);
  try {
    const created = await api("POST", "/api/requests", {
      category: form.get("category"), title: form.get("title"), description: form.get("description"),
    });
    event.target.reset();
    show(`Request submitted. Status: ${created.status}.`);
    await refresh();
  } catch (error) {
    show("Could not submit: " + error.message, true);
  }
});

// ---- List -------------------------------------------------------------------------------------
$("status-filter").addEventListener("change", refresh);
$("overdue-filter").addEventListener("change", refresh);

async function refresh() {
  const status = $("status-filter").value;
  const overdue = $("overdue-filter").value;
  const filters = new URLSearchParams();
  if (status) filters.set("status", status);
  if (overdue) filters.set("overdue", overdue);
  const query = filters.size ? `?${filters}` : "";
  const requests = await api("GET", "/api/requests" + query);

  const rows = requests.map((request) => {
    const row = document.createElement("tr");
    for (const value of [request.title, request.category,
                         request.status + (request.isOverdue ? " (Overdue)" : ""), request.assigneeId ?? "—",
                         new Date(request.submittedAt).toLocaleString(), new Date(request.dueAt).toLocaleString()]) {
      const cell = document.createElement("td");
      cell.textContent = value;
      row.append(cell);
    }
    const open = document.createElement("button");
    open.textContent = "View";
    open.addEventListener("click", () => openDetail(request.id));
    const actionCell = document.createElement("td");
    actionCell.append(open);
    row.append(actionCell);
    return row;
  });
  $("request-rows").replaceChildren(...rows);
  $("empty-list").hidden = requests.length > 0;

  if (currentUser.role === "Manager") {
    const counts = await api("GET", "/api/reports/counts" + query);
    $("count-open").textContent = counts.open;
    $("count-rejected").textContent = counts.rejected;
    $("count-resolved").textContent = counts.resolved;
    $("count-overdue").textContent = counts.overdue;
  }
}

// ---- Detail and staff actions -----------------------------------------------------------------
async function openDetail(id) {
  const detail = await api("GET", `/api/requests/${id}`);
  openRequestId = id;
  $("detail-title").textContent = detail.request.title;
  $("detail-status").textContent = detail.request.status;
  $("detail-category").textContent = detail.request.category;
  $("detail-description").textContent = detail.description;
  $("detail-deadline").textContent = `Due: ${new Date(detail.request.dueAt).toLocaleString()}` +
    (detail.request.isOverdue ? " — Overdue" : "");
  $("staff-actions").hidden = currentUser.role !== "Staff";

  const items = detail.history.map((entry) => {
    const item = document.createElement("li");
    const extra = entry.reason ? ` — reason: ${entry.reason}`
                : entry.resolutionNote ? ` — note: ${entry.resolutionNote}`
                : entry.assigneeId ? ` — assigned to ${entry.assigneeId}` : "";
    item.textContent = `${new Date(entry.at).toLocaleString()}: ${entry.from} → ${entry.to} by ${entry.actorId}${extra}`;
    return item;
  });
  if (items.length === 0) items.push(Object.assign(document.createElement("li"), { textContent: "Submitted; no changes yet." }));
  $("detail-history").replaceChildren(...items);
  $("detail-view").hidden = false;
}

$("close-detail").addEventListener("click", () => { $("detail-view").hidden = true; });

$("staff-actions").addEventListener("click", async (event) => {
  const action = event.target.dataset.action;
  if (!action) return;

  const body =
    action === "assign" ? { to: "Assigned", assigneeId: currentUser.id } :
    action === "reject" ? { to: "Rejected", reason: prompt("Reason for rejection (shown to the requester):") } :
                          { to: "Resolved", resolutionNote: prompt("Resolution note:") };
  try {
    const result = await api("POST", `/api/requests/${openRequestId}/transitions`, body);
    show(`Status changed to ${result.status}.`);
    await openDetail(openRequestId);
    await refresh();
  } catch (error) {
    show("Status change refused: " + error.message, true);
  }
});

// ---- Resume an existing session on page load ----------------------------------------------------
api("GET", "/api/auth/me")
  .then((user) => { currentUser = user; return startSession(); })
  .catch(() => { /* not signed in: the login form is already showing */ });
