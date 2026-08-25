let accessToken = ''

const bearerAuthentication = {
  token: '',
}

const scalarFetch = window.fetch.bind(window)

const isLoginRequest = (input) => {
  if (!input) {
    return false
  }

  const url = input instanceof Request ? input.url : input.toString()
  const path = new URL(url, window.location.origin).pathname.replace(/\/+$/, '')

  return path.toLowerCase().endsWith('/api/identity/login')
}

const getRequestInit = (input, init) => {
  const headers = new Headers(
    init?.headers ?? (input instanceof Request ? input.headers : undefined),
  )

  if (isLoginRequest(input)) {
    headers.delete('Authorization')
  } else if (accessToken) {
    headers.set('Authorization', `Bearer ${accessToken}`)
  }

  return {
    ...init,
    headers,
  }
}

const setBearerAuthentication = (token) => {
  accessToken = token
  bearerAuthentication.token = token

  for (const input of document.querySelectorAll('input[placeholder="Token"]')) {
    input.value = token
    input.dispatchEvent(new Event('input', { bubbles: true }))
    input.dispatchEvent(new Event('change', { bubbles: true }))
  }
}

window.fetch = async (input, init) => {
  const response = await scalarFetch(input, getRequestInit(input, init))

  if (response.ok && isLoginRequest(input)) {
    try {
      const result = await response.clone().json()
      const loginAccessToken = result?.data?.accessToken

      if (typeof loginAccessToken === 'string' && loginAccessToken) {
        setBearerAuthentication(loginAccessToken)
      }
    } catch {
      // Ignore successful non-JSON responses.
    }
  }

  return response
}

const scalarConfiguration = {
  authentication: {
    preferredSecurityScheme: 'Bearer',
    securitySchemes: {
      Bearer: bearerAuthentication,
    },
  },
  tagsSorter: (left, right) => {
    const leftOrder = left['x-order'] ?? Number.MAX_SAFE_INTEGER
    const rightOrder = right['x-order'] ?? Number.MAX_SAFE_INTEGER

    return leftOrder - rightOrder || left.name.localeCompare(right.name)
  },
}

export default scalarConfiguration
