export function roleGuard(role) {

  return (to, from, next) => {

    const user = JSON.parse(localStorage.getItem("user"))

    if (!user) return next("/login")

    if (user.role !== role) {

      return next("/forbidden")

    }

    next()

  }

}
