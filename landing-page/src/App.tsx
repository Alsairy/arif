import { LanguageProvider } from './contexts/LanguageContext'
import Navigation from './components/Navigation'
import Hero from './components/Hero'
import Features from './components/Features'
import Solutions from './components/Solutions'
import Testimonials from './components/Testimonials'
import CTA from './components/CTA'
import DemoForm from './components/DemoForm'
import Footer from './components/Footer'

function App() {
  return (
    <LanguageProvider>
      <div className="min-h-screen bg-white">
        <Navigation />
        <main>
          <section id="hero">
            <Hero />
          </section>
          <section id="features">
            <Features />
          </section>
          <section id="solutions">
            <Solutions />
          </section>
          <section id="testimonials">
            <Testimonials />
          </section>
          <section id="cta">
            <CTA />
          </section>
          <section id="demo-form">
            <DemoForm />
          </section>
        </main>
        <Footer />
      </div>
    </LanguageProvider>
  )
}

export default App
