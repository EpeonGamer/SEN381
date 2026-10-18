// All request data is rendered as text. The server remains the authorization boundary.
const $ = id => document.getElementById(id);
let currentUser = null;
let openRequestId = null;
let openVersion = null;
let openStatus = null;
let openAssigneeId = null;
let page = 0;
let refreshVersion = 0;
let detailVersion = 0;
let returnFocus = null;
let appliedSearch = '';
const pageSize = 5;
const messages = {
  NotAuthorised: 'You do not have permission to perform this action.',
  NotAssigned: 'You can only resolve a request when you are assigned to it. Reassign the request to yourself first.',
  InvalidInput: 'Check the required fields and try again.',
  InvalidCategory: 'Choose a valid category.',
  InvalidAssignee: 'Choose a staff member authorised for this category.',
  InvalidState: 'The request is no longer in the required state. Open it again.',
  MissingAssignee: 'Choose a staff member.',
  MissingReason: 'Enter a reason for rejection.',
  MissingResolutionNote: 'Enter a resolution note.',
  RequestNotFound: 'The request is unavailable or you do not have access.',
  InvalidTransition: 'This status change is not allowed. Open the request again.',
  ConcurrencyConflict: 'Someone changed the request. Open it again before retrying.',
  InvalidCredentials: 'The user ID or password is incorrect.'
};

async function api(method, path, body) {
  let response;
  try {
    response = await fetch(path, {
      method, credentials: 'same-origin',
      headers: body === undefined ? {} : { 'Content-Type': 'application/json' },
      body: body === undefined ? undefined : JSON.stringify(body)
    });
  } catch {
    throw new Error('Cannot reach the server. Check your connection before retrying.');
  }
  const data = response.status === 204 ? null : await response.json().catch(() => null);
  if (!response.ok) {
    if (response.status === 401 && path !== '/api/auth/login') $('login-view').hidden = false;
    const fallback = {
      400: messages.InvalidInput, 401: 'Your session expired or login was refused. Please log in again.',
      403: messages.NotAuthorised, 404: messages.RequestNotFound, 409: messages.ConcurrencyConflict,
      429: 'Too many attempts. Wait a moment and try again.'
    }[response.status] || 'The request could not be completed. Please try again.';
    // Only known application codes are trusted; malformed/proxy responses never expose internals.
    const message = response.status >= 500
      ? 'The server could not complete the operation. Refresh to check its outcome before retrying.'
      : messages[data?.error] || fallback;
    throw Object.assign(new Error(message), { status: response.status });
  }
  if (response.status !== 204 && data === null)
    throw new Error('The server returned an unreadable response. Refresh and try again.');
  return data;
}

function show(text, isError = false) {
  $('message').textContent = text;
  $('message').className = isError ? 'error' : 'ok';
  if (isError) {
    $('error-text').textContent = text;
    if (!$('error-dialog').open) $('error-dialog').showModal();
  }
}
async function run(action) {
  try { await action(); } catch (error) { show(error.message, true); }
}
// Lock the whole action region, including alternative actions, until the write and refresh finish.
async function submitOnce(region, action) {
  if (region.dataset.busy) return;
  region.dataset.busy = 'true';
  const controls = [...region.querySelectorAll('button, input, select, textarea')];
  const states = controls.map(control => control.disabled);
  controls.forEach(control => { control.disabled = true; });
  try { await action(); }
  catch (error) { show(error.message, true); }
  finally {
    controls.forEach((control, index) => { control.disabled = states[index]; });
    delete region.dataset.busy;
  }
}
document.addEventListener('invalid', event => {
  event.preventDefault();
  show(event.target.validationMessage, true);
}, true);

$('login-form').addEventListener('submit', event => {
  event.preventDefault();
  const form = new FormData(event.target);
  return submitOnce(event.target, async () => {
    currentUser = await api('POST', '/api/auth/login', {
      userId: form.get('userId'), password: form.get('password')
    });
    event.target.reset();
    show('');
    await startSession();
  });
});
$('logout').addEventListener('click', () => run(async () => {
  await api('POST', '/api/auth/logout');
  location.reload();
}));
async function startSession() {
  ++detailVersion;
  openRequestId = null;
  $('detail-view').hidden = true;
  $('request-rows').replaceChildren();
  $('who').textContent = currentUser.id;
  $('role').textContent = currentUser.role;
  $('session').hidden = false;
  $('login-view').hidden = true;
  $('list-view').hidden = false;
  $('submit-view').hidden = currentUser.role !== 'Requester';
  $('counts-view').hidden = currentUser.role !== 'Manager';
  $('list-title').textContent = currentUser.role === 'Requester' ? 'My requests' : 'Requests';
  const categories = await api('GET', '/api/categories');
  const selectedCategory = $('category').value;
  const categoryFilter = $('category-filter').value;
  $('category').replaceChildren(...categories.map(name => new Option(name, name)));
  $('category-filter').replaceChildren(new Option('All categories', ''), ...categories.map(name => new Option(name, name)));
  if (categories.includes(selectedCategory)) $('category').value = selectedCategory;
  if (categories.includes(categoryFilter)) $('category-filter').value = categoryFilter;
  await refresh();
}
$('submit-form').addEventListener('submit', event => {
  event.preventDefault();
  const form = new FormData(event.target);
  return submitOnce(event.target, async () => {
    const created = await api('POST', '/api/requests', {
      category: form.get('category'), title: form.get('title'), description: form.get('description')
    });
    event.target.reset();
    show(`Request submitted. Status: ${created.status}.`);
    page = 0;
    await refresh();
  });
});

function filters() {
  const query = new URLSearchParams();
  for (const key of ['category', 'status', 'overdue']) {
    const value = $(key + '-filter').value;
    if (value) query.set(key, value);
  }
  if (appliedSearch) query.set('search', appliedSearch);
  if ($('sort-filter').value) query.set('sortByCategory', 'true');
  return query;
}
function resetPage() { page = 0; return run(refresh); }
for (const key of ['category', 'status', 'overdue', 'sort']) $(key + '-filter').addEventListener('change', resetPage);
$('search-form').addEventListener('submit', event => {
  event.preventDefault();
  appliedSearch = $('search-input').value.trim();
  resetPage();
});
$('clear-filters').addEventListener('click', () => {
  for (const key of ['category', 'status', 'overdue', 'sort']) $(key + '-filter').value = '';
  $('search-input').value = appliedSearch = '';
  resetPage();
});
$('previous-page').addEventListener('click', () => { if (page > 0) { page--; run(refresh); } });
$('next-page').addEventListener('click', () => { page++; run(refresh); });
async function refresh() {
  const version = ++refreshVersion;
  const query = filters();
  const reportQuery = query.toString();
  query.set('limit', pageSize + 1); // one look-ahead row, with filtering applied before pagination
  query.set('offset', page * pageSize);
  $('list-loading').hidden = false;
  $('list-view').setAttribute('aria-busy', 'true');
  $('previous-page').disabled = $('next-page').disabled = true;
  try {
    const [requests, counts] = await Promise.all([
      api('GET', '/api/requests?' + query),
      currentUser.role === 'Manager' ? api('GET', '/api/reports/counts?' + reportQuery) : null
    ]);
    if (version !== refreshVersion) return;
    if (!requests.length && page > 0) { page--; return await refresh(); }
    const rows = requests.slice(0, pageSize).map(request => {
      const row = document.createElement('tr');
      for (const value of [request.title, request.category,
        request.status + (request.isOverdue ? ' (Overdue)' : ''), request.assigneeId ?? '—',
        new Date(request.submittedAt).toLocaleString(), new Date(request.dueAt).toLocaleString()]) {
        const cell = document.createElement('td');
        cell.textContent = value;
        row.append(cell);
      }
      const open = document.createElement('button');
      open.textContent = 'View';
      open.setAttribute('aria-label', 'View request: ' + request.title);
      open.addEventListener('click', () => { returnFocus = open; run(() => openDetail(request.id)); });
      const cell = document.createElement('td');
      cell.append(open);
      row.append(cell);
      return row;
    });
    $('request-rows').replaceChildren(...rows);
    $('empty-list').hidden = requests.length > 0;
    $('page-label').textContent = `Page ${page + 1} · ${rows.length} shown`;
    $('previous-page').disabled = page === 0;
    $('next-page').disabled = requests.length <= pageSize;
    if (counts) for (const key of ['open', 'rejected', 'resolved', 'overdue']) $('count-' + key).textContent = counts[key];
  } catch (error) {
    if (version !== refreshVersion) return;
    $('request-rows').replaceChildren();
    $('empty-list').hidden = true;
    $('page-label').textContent = 'Requests unavailable. Search again to retry.';
    throw error;
  } finally {
    if (version === refreshVersion) {
      $('list-loading').hidden = true;
      $('list-view').removeAttribute('aria-busy');
    }
  }
}

function listItems(id, entries, render, empty) {
  const items = entries.map(entry => Object.assign(document.createElement('li'), { textContent: render(entry) }));
  $(id).replaceChildren(...(items.length ? items : [Object.assign(document.createElement('li'), { textContent: empty })]));
}
async function openDetail(id, focus = true) {
  const version = ++detailVersion;
  openRequestId = null;
  openVersion = null;
  $('detail-content').hidden = true;
  $('detail-title').textContent = 'Request details';
  $('detail-view').hidden = false;
  $('detail-loading').hidden = false;
  $('staff-actions').hidden = true;
  if (focus) $('detail-title').focus();
  try {
    const detail = await api('GET', `/api/requests/${id}`);
    const staff = currentUser.role === 'Staff' && ['Unassigned', 'Assigned'].includes(detail.request.status)
      ? await api('GET', `/api/requests/${id}/eligible-assignees`) : [];
    if (version !== detailVersion) return;
    openRequestId = id;
    openVersion = detail.request.version;
    openStatus = detail.request.status;
    openAssigneeId = detail.request.assigneeId;
    $('detail-title').textContent = detail.request.title;
    $('detail-status').textContent = openStatus;
    $('detail-category').textContent = detail.request.category;
    $('detail-assignee').textContent = openAssigneeId ?? 'Unassigned';
    $('detail-description').textContent = detail.description;
    $('detail-deadline').textContent = `Due: ${new Date(detail.request.dueAt).toLocaleString()}` + (detail.request.isOverdue ? ' — Overdue' : '');
    $('staff-actions').hidden = currentUser.role !== 'Staff';
    for (const button of $('staff-actions').querySelectorAll('[data-action]')) {
      button.hidden = button.dataset.action === 'resolve' ? openStatus !== 'Assigned' : openStatus !== 'Unassigned';
    }
    const others = openStatus === 'Assigned' ? staff : staff.filter(name => name !== currentUser.id);
    $('assign-other-form').hidden = !['Assigned', 'Unassigned'].includes(openStatus) || !others.length;
    $('other-assignee').replaceChildren(new Option('Choose staff member', ''),
      ...others.map(name => new Option(name === currentUser.id ? `${name} (me)` : name, name)));
    $('assign-other-button').textContent = openStatus === 'Assigned' ? 'Reassign request' : 'Assign request';
    $('assignee-hint').textContent = ['Assigned', 'Unassigned'].includes(openStatus) && !others.length
      ? 'No other eligible staff are configured for this category.' : '';
    $('comment-form').reset();
    listItems('detail-history', detail.history, entry => {
      const extra = entry.reason ? ` — reason: ${entry.reason}` : entry.resolutionNote ? ` — note: ${entry.resolutionNote}` : entry.assigneeId ? ` — assigned to ${entry.assigneeId}` : '';
      return `${new Date(entry.at).toLocaleString()}: ${entry.from} → ${entry.to} by ${entry.actorId}${extra}`;
    }, 'Submitted; no status changes yet.');
    listItems('detail-comments', [...(detail.comments ?? [])].sort((a, b) => new Date(a.at) - new Date(b.at)),
      entry => `${new Date(entry.at).toLocaleString()} — ${entry.authorId}: ${entry.content || '(Empty comment)'}`, 'No staff comments yet.');
    $('detail-content').hidden = false;
  } catch (error) {
    if (version !== detailVersion) return;
    $('detail-title').textContent = 'Request unavailable';
    throw error;
  } finally { if (version === detailVersion) $('detail-loading').hidden = true; }
}
$('close-detail').addEventListener('click', () => {
  ++detailVersion;
  openRequestId = null;
  $('detail-view').hidden = true;
  (returnFocus?.isConnected ? returnFocus : $('list-title')).focus();
});
async function afterWrite(id, text) {
  show(text);
  // Do not reopen a detail that was closed or replaced while saving.
  await refresh();
  if (openRequestId === id) await openDetail(id, false);
}
$('staff-actions').addEventListener('click', event => {
  const action = event.target.dataset.action;
  if (!action || !openRequestId) return;
  if (action === 'resolve' && openAssigneeId !== currentUser.id) {
    show(messages.NotAssigned, true);
    return;
  }
  const id = openRequestId;
  const note = action === 'assign' ? null : prompt(action === 'reject' ? 'Reason for rejection (shown to the requester):' : 'Resolution note:');
  if (action !== 'assign' && note === null) return;
  return submitOnce($('staff-actions'), async () => {
    const body = action === 'assign' ? { to: 'Assigned', assigneeId: currentUser.id }
      : action === 'reject' ? { to: 'Rejected', reason: note } : { to: 'Resolved', resolutionNote: note };
    const result = await api('POST', `/api/requests/${id}/transitions`, body);
    await afterWrite(id, `Status changed to ${result.status}.`);
  });
});
$('assign-other-form').addEventListener('submit', event => {
  event.preventDefault();
  if (!openRequestId) return;
  const id = openRequestId;
  const assigneeId = $('other-assignee').value;
  const expectedVersion = openVersion;
  const status = openStatus;
  return submitOnce($('staff-actions'), async () => {
    if (status === 'Unassigned') await api('POST', `/api/requests/${id}/transitions`, { to: 'Assigned', assigneeId });
    else await api('POST', `/api/requests/${id}/reassign`, { assigneeId, expectedVersion });
    await afterWrite(id, `Request assigned to ${assigneeId}.`);
  });
});
$('comment-form').addEventListener('submit', event => {
  event.preventDefault();
  if (!openRequestId) return;
  const id = openRequestId;
  const content = $('comment-content').value;
  return submitOnce($('staff-actions'), async () => {
    await api('POST', `/api/requests/${id}/comments`, { content });
    await afterWrite(id, 'Comment saved.');
  });
});
api('GET', '/api/auth/me').then(user => { currentUser = user; return startSession(); })
  .catch(error => { if (error.status !== 401) show(error.message, true); });
